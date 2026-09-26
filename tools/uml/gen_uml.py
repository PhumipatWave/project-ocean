"""Generate Mermaid UML docs for Assets/Project_Ocean/Scripts into docs/uml/.

Static regex parser (no compiler): links made only through GetComponent<T>,
inspector wiring, prefabs or strings are invisible. Run: python tools/uml/gen_uml.py
"""
import re
import sys
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "Assets" / "Project_Ocean" / "Scripts"
OUT = ROOT / "docs" / "uml"
EXCLUDE = {"InputSystem_Actions.cs"}
LAYER_RANK = {"Core": 0, "Gameplay": 1, "UI": 2, "Editor": 3}
UNITY_BASES = {"MonoBehaviour": "MB", "ScriptableObject": "SO", "Editor": "Editor",
               "PropertyDrawer": "Drawer", "EditorWindow": "EditorWindow"}
MAX_FIELDS, MAX_METHODS = 10, 12
SPLIT_MODULE_OVER = 25
MIN_OVERVIEW_EDGE = 3
MODS = r"(?:public|private|protected|internal|static|abstract|sealed|partial|readonly|virtual|override|new|async|extern|unsafe|const|volatile|event)"


# ---------- lexing ----------

def strip_code(text):
    """Blank out comments, strings and chars (keeping newlines) so regexes see only code."""
    out, i, n = [], 0, len(text)
    while i < n:
        c, nxt = text[i], text[i + 1] if i + 1 < n else ""
        if c == "/" and nxt == "/":
            while i < n and text[i] != "\n":
                i += 1
        elif c == "/" and nxt == "*":
            j = text.find("*/", i + 2)
            j = n if j < 0 else j + 2
            out.append(re.sub(r"[^\n]", " ", text[i:j]))
            i = j
        elif c == '"' or (c in "@$" and nxt == '"') or (c in "@$" and nxt in "@$" and text[i + 2:i + 3] == '"'):
            start = i
            verbatim = False
            while text[i] != '"':
                verbatim |= text[i] == "@"
                i += 1
            i += 1
            while i < n:
                if verbatim and text[i] == '"' and text[i + 1:i + 2] == '"':
                    i += 2
                    continue
                if not verbatim and text[i] == "\\":
                    i += 2
                    continue
                if text[i] == '"':
                    i += 1
                    break
                i += 1
            out.append('""' + re.sub(r"[^\n]", "", text[start:i]))
        elif c == "'":
            j = i + 1
            while j < n and text[j] != "'":
                j += 2 if text[j] == "\\" else 1
            out.append("' '")
            i = j + 1
        else:
            out.append(c)
            i += 1
    return "".join(out)


def match_brace(s, open_idx):
    depth = 0
    for k in range(open_idx, len(s)):
        if s[k] == "{":
            depth += 1
        elif s[k] == "}":
            depth -= 1
            if depth == 0:
                return k
    return len(s) - 1


def split_top(s, sep=","):
    parts, depth, cur = [], 0, []
    for ch in s:
        if ch in "<([":
            depth += 1
        elif ch in ">)]":
            depth -= 1
        if ch == sep and depth == 0:
            parts.append("".join(cur).strip())
            cur = []
        else:
            cur.append(ch)
    if "".join(cur).strip():
        parts.append("".join(cur).strip())
    return parts


# ---------- model ----------

class Member:
    def __init__(self, kind, name, typ="", vis="private", static=False, abstract=False, params=""):
        self.kind, self.name, self.type, self.vis = kind, name, typ, vis
        self.static, self.abstract, self.params = static, abstract, params


class TypeInfo:
    def __init__(self, name, kind, mods, bases, file, line, loc, body):
        self.name, self.kind, self.mods, self.bases = name, kind, mods, bases
        self.file, self.line, self.loc, self.body = file, line, loc, body
        self.members, self.enum_values = [], []
        self.base_class, self.interfaces, self.stereo = None, [], []
        self.assoc = {}          # target -> member name
        self.deps = set()
        self.subs = []           # (publisher, event)
        self.outer = None        # enclosing type for nested types

    @property
    def module(self):
        return str(self.file.parent.relative_to(SRC)).replace("\\", "/") if self.file.parent != SRC else "(root)"

    @property
    def layer(self):
        return self.module.split("/")[0]


TYPE_RE = re.compile(
    r"(?P<mods>(?:\b(?:public|internal|private|protected|static|abstract|sealed|partial|readonly|ref)\s+)*)"
    r"\b(?P<kind>class|struct|interface|enum|record(?:\s+struct|\s+class)?)\s+(?P<name>[A-Za-z_]\w*)"
    r"(?P<gen>\s*<[^>{;]*>)?\s*(?:\([^)]*\))?\s*(?::\s*(?P<bases>[^{;]+?))?\s*(?:where\s[^{]*)?\{")


def parse_types(code, file, offset=0, outer=None):
    """Find type declarations in code; returns list of TypeInfo (nested types included)."""
    found, pos = [], 0
    while True:
        m = TYPE_RE.search(code, pos)
        if not m:
            break
        open_idx = m.end() - 1
        close_idx = match_brace(code, open_idx)
        body = code[open_idx + 1:close_idx]
        abs_start = offset + m.start()
        line = code.count("\n", 0, m.start()) + 1 + (outer or 0)
        loc = sum(1 for l in code[m.start():close_idx + 1].splitlines() if l.strip())
        kind = m.group("kind").split()[0]
        t = TypeInfo(m.group("name"), kind, m.group("mods").split(), split_top(m.group("bases") or ""),
                     file, line, loc, body)
        found.append(t)
        nested = parse_types(body, file, abs_start, line - 1)
        found.extend(nested)
        t.own_body = body
        for nt in nested:
            nt.outer = nt.outer or t.name
            t.own_body = t.own_body.replace(nt.body, "")
        pos = close_idx + 1
    return found


def member_headers(body):
    """Yield top-level member declaration headers of a type body."""
    i, n, start = 0, len(body), 0
    while i < n:
        ch = body[i]
        if ch == "(":
            depth = 0
            while i < n:
                depth += body[i] == "("
                depth -= body[i] == ")"
                i += 1
                if depth == 0:
                    break
            continue
        if ch == ";":
            yield body[start:i]
            start = i + 1
        elif ch == "{":
            yield body[start:i] + " {"
            i = match_brace(body, i)
            start = i + 1
        i += 1


def parse_members(t):
    if t.kind == "enum":
        t.enum_values = [re.sub(r"\s*=.*", "", v).strip() for v in split_top(t.own_body) if v.strip()]
        t.enum_values = [v for v in t.enum_values if re.fullmatch(r"\w+", v)]
        return
    for raw in member_headers(t.own_body):
        h = re.sub(r"\[[^\[\]]*(?:\[[^\]]*\][^\[\]]*)*\]", " ", raw)   # attributes
        h = " ".join(h.split())
        if not h or h.startswith(("using ", "#")) or re.search(r"\b(class|struct|interface|enum|record|delegate)\b", h):
            continue
        has_block = h.endswith(" {")
        h = h[:-2].strip() if has_block else h
        arrow = h.find("=>")
        head = h[:arrow].strip() if arrow >= 0 else h
        mods = set(re.findall(r"\b(public|private|protected|internal|static|abstract|virtual|override|const|event)\b",
                              head.split("(")[0]))
        vis = next((v for v in ("public", "protected", "internal") if v in mods), "private")
        if t.kind == "interface":
            vis = "public"
        static = "static" in mods or "const" in mods
        abstract = "abstract" in mods or t.kind == "interface"
        body_head = re.sub(r"^(?:" + MODS + r"\s+)*", "", head)
        if "event" in mods:
            m = re.match(r"(.+?)\s+(\w+)\s*(?:=.*)?$", body_head)
            if m:
                t.members.append(Member("event", m.group(2), m.group(1), vis, static))
            continue
        m = re.match(r"(?:(?P<ret>[\w<>\[\],.?\s()]+?)\s+)?(?P<name>~?\w+|operator\s*\S+|this)\s*(?P<gen><[^()]*>)?\s*\((?P<params>.*)\)\s*(?::\s*(?:base|this)\s*\(.*)?$", body_head)
        if m and "=" not in body_head.split("(")[0]:
            name, ret = m.group("name"), (m.group("ret") or "").strip()
            if name == t.name or name.startswith("~") or ret in ("", "implicit operator", "explicit operator"):
                kind = "ctor" if name == t.name else "method"
            else:
                kind = "method"
            params = [p.split("=")[0].strip() for p in split_top(m.group("params"))]
            ptypes = ", ".join(" ".join(p.split()[:-1]) or p for p in params if p)
            t.members.append(Member(kind, name, ret, vis, static, abstract and kind == "method", ptypes))
            continue
        decl = body_head.split("=")[0].strip() if not has_block else body_head
        m = re.match(r"(.+?)\s+(\w+(?:\s*,\s*\w+)*)$", decl)
        if not m:
            continue
        typ, names = m.group(1).strip(), [x.strip() for x in m.group(2).split(",")]
        kind = "property" if has_block or arrow >= 0 else "field"
        if typ in ("return", "using", "namespace", "else", "new"):
            continue
        for nm in names:
            t.members.append(Member(kind, nm, typ, vis, static, False))


# ---------- analysis ----------

IDENT = re.compile(r"\b[A-Z]\w*\b")


def load():
    types, files = [], {}
    for f in sorted(SRC.rglob("*.cs")):
        if f.name in EXCLUDE or "Deprecated" in f.relative_to(SRC).parts:
            continue
        raw = f.read_text(encoding="utf-8-sig", errors="replace")
        code = strip_code(raw)
        files[f] = (raw, code)
        for t in parse_types(code, f):
            parse_members(t)
            types.append(t)
    return types, files


def resolve(types):
    counts = defaultdict(int)
    for t in types:
        counts[t.name] += 1
    for t in types:
        if counts[t.name] > 1 and t.outer:
            t.name = f"{t.outer}_{t.name}"
    by_name = {}
    dupes = set()
    for t in types:
        if t.name in by_name:
            dupes.add(t.name)
        by_name.setdefault(t.name, t)
    names = set(by_name)
    events = defaultdict(set)
    for t in types:
        for mb in t.members:
            if mb.kind == "event":
                events[mb.name].add(t.name)
    for t in types:
        for i, b in enumerate(t.bases):
            bn = re.sub(r"<.*", "", b).split(".")[-1].strip()
            if bn in UNITY_BASES:
                t.stereo.append(UNITY_BASES[bn])
            elif bn in by_name:
                if by_name[bn].kind == "interface":
                    t.interfaces.append(bn)
                elif i == 0 and t.kind in ("class", "record"):
                    t.base_class = bn
            elif i == 0 and t.kind == "class" and not bn.startswith("I"):
                t.stereo.append(bn)
        if "static" in t.mods:
            t.stereo.append("static")
        if "abstract" in t.mods:
            t.stereo.append("abstract")
        if t.kind in ("interface", "enum", "struct"):
            t.stereo.append(t.kind)
        # SO/MB through a project base class
        for mb in t.members:
            if mb.kind in ("field", "property"):
                for ident in IDENT.findall(mb.type):
                    if ident in names and ident != t.name and ident not in t.assoc:
                        t.assoc[ident] = mb.name
        used = set(IDENT.findall(t.body)) & names
        t.deps = used - {t.name} - set(t.assoc) - {t.base_class} - set(t.interfaces)
        for m in re.finditer(r"([\w.]+?)\.(\w+)\s*\+=", t.body):
            chain, ev = m.group(1).split("."), m.group(2)
            if ev not in events:
                continue
            pubs = events[ev]
            pub = next((c for c in chain if c in pubs), None)
            if pub is None:
                field_types = {mb.name: mb.type for mb in t.members if mb.kind in ("field", "property")}
                ft = field_types.get(chain[-1], "")
                pub = next((p for p in pubs if re.search(rf"\b{p}\b", ft)), None)
            if pub is None and len(pubs) == 1:
                pub = next(iter(pubs))
            if pub is None and t.name in pubs:
                pub = t.name
            if pub and (pub, ev) not in t.subs:
                t.subs.append((pub, ev))
    # inherit MB/SO stereotype through project bases
    for t in types:
        cur, seen = t, set()
        while cur.base_class and cur.base_class not in seen and not ({"MB", "SO"} & set(t.stereo)):
            seen.add(cur.base_class)
            cur = by_name[cur.base_class]
            for s in ("MB", "SO"):
                if s in cur.stereo and s not in t.stereo:
                    t.stereo.append(s)
    return by_name, dupes


def edges_of(t):
    return set(t.assoc) | t.deps | ({t.base_class} if t.base_class else set()) | set(t.interfaces) | {p for p, _ in t.subs}


def tarjan(nodes, adj):
    index, low, stack, on, out, counter = {}, {}, [], set(), [], [0]
    sys.setrecursionlimit(10000)

    def visit(v):
        index[v] = low[v] = counter[0]
        counter[0] += 1
        stack.append(v)
        on.add(v)
        for w in adj.get(v, ()):
            if w not in index:
                visit(w)
                low[v] = min(low[v], low[w])
            elif w in on:
                low[v] = min(low[v], index[w])
        if low[v] == index[v]:
            comp = []
            while True:
                w = stack.pop()
                on.discard(w)
                comp.append(w)
                if w == v:
                    break
            if len(comp) > 1:
                out.append(sorted(comp))

    for v in nodes:
        if v not in index:
            visit(v)
    return sorted(out, key=len, reverse=True)


def wrong_way(a_layer, b_layer):
    ra, rb = LAYER_RANK.get(a_layer), LAYER_RANK.get(b_layer)
    return ra is not None and rb is not None and ra < rb


# ---------- mermaid rendering ----------

def mm(s):
    """Mermaid-safe type text: generics as ~T~, drop chars the classDiagram parser rejects."""
    s = s.replace("<", "~").replace(">", "~").replace("[]", "Array").replace("?", "")
    return re.sub(r"[{}:;\"#]", "", s)


def nid(s):
    return re.sub(r"\W", "_", s)


VIS = {"public": "+", "protected": "#", "internal": "~", "private": "-"}


def class_block(t, full_api=True):
    lines = [f"  class {t.name} {{"]
    for s in t.stereo[:2]:
        lines.append(f"    <<{s}>>")
    if t.kind == "enum":
        vals = t.enum_values
        lines += [f"    {v}" for v in vals[:MAX_FIELDS]]
        if len(vals) > MAX_FIELDS:
            lines.append(f"    ... +{len(vals) - MAX_FIELDS} more")
    elif full_api:
        api = [m for m in t.members if m.vis in ("public", "protected")]
        fields = [m for m in api if m.kind in ("field", "property", "event")]
        methods = [m for m in api if m.kind in ("method", "ctor")]
        for m in fields[:MAX_FIELDS]:
            ev = "event " if m.kind == "event" else ""
            lines.append(f"    {VIS[m.vis]}{ev}{mm(m.type)} {m.name}{'$' if m.static else ''}")
        if len(fields) > MAX_FIELDS:
            lines.append(f"    ... +{len(fields) - MAX_FIELDS} more fields")
        for m in methods[:MAX_METHODS]:
            ret = "" if m.kind == "ctor" else " " + mm(m.type)
            suffix = "$" if m.static else ("*" if m.abstract else "")
            lines.append(f"    {VIS[m.vis]}{m.name}({mm(m.params)}){suffix}{ret}")
        if len(methods) > MAX_METHODS:
            lines.append(f"    ... +{len(methods) - MAX_METHODS} more methods()")
    lines.append("  }")
    return lines


def class_diagram(ts, by_name, ext_label=True):
    inside = {t.name for t in ts}
    out = ["```mermaid", "classDiagram", "  direction LR"]
    ext = {}
    rels = []
    for t in ts:
        out += class_block(t)
        if t.base_class:
            rels.append(f"  {t.base_class} <|-- {t.name}")
            if t.base_class not in inside:
                ext[t.base_class] = by_name[t.base_class].module
        for i in t.interfaces:
            rels.append(f"  {i} <|.. {t.name}")
            if i not in inside:
                ext[i] = by_name[i].module
        for target, member in sorted(t.assoc.items()):
            if target in inside:
                rels.append(f"  {t.name} --> {target} : {member}")
    for name, mod in sorted(ext.items()):
        out += [f"  class {name} {{", f"    <<{mod}>>", "  }"] if ext_label else [f"  class {name}"]
    out += sorted(set(rels))
    out.append("```")
    return out


def md_link_module(mod):
    return f"modules/{nid(mod)}.md"


# ---------- pages ----------

def page_modules(types, by_name, modules):
    written = []
    (OUT / "modules").mkdir(parents=True, exist_ok=True)
    for mod, ts in sorted(modules.items()):
        chunks = [(None, ts)]
        if len(ts) > SPLIT_MODULE_OVER:
            groups = defaultdict(list)
            for t in ts:
                groups[t.file.stem].append(t)
            chunks, cur, size = [], [], 0
            for _, g in sorted(groups.items()):
                if cur and size + len(g) > SPLIT_MODULE_OVER:
                    chunks.append((f"part {len(chunks) + 1}", cur))
                    cur, size = [], 0
                cur += g
                size += len(g)
            chunks.append((f"part {len(chunks) + 1}", cur))
        loc = sum(t.loc for t in ts if not t.outer)
        depends = defaultdict(int)
        for t in ts:
            for e in edges_of(t):
                if e in by_name and by_name[e].module != mod:
                    depends[by_name[e].module] += 1
        dep_txt = ", ".join(f"[{m}]({nid(m)}.md) ×{c}" for m, c in sorted(depends.items(), key=lambda x: -x[1])) or "none"
        lines = [f"# {mod}", "", f"{len(ts)} types · {loc} LOC (non-blank) · depends on: {dep_txt}", "",
                 "[← index](../README.md)", ""]
        for title, part in chunks:
            if title:
                lines += [f"## {title}", ""]
            lines += class_diagram(part, by_name) + [""]
        subs = [(t.name, p, e) for t in ts for p, e in t.subs]
        if subs:
            lines += ["## Event subscriptions", "", "| Subscriber | Publisher | Event |", "|---|---|---|"]
            lines += [f"| {s} | {p} | {e} |" for s, p, e in sorted(subs)] + [""]
        lines += ["## Files", "", "| Type | Kind | File | LOC |", "|---|---|---|---|"]
        lines += [f"| {t.name} | {', '.join(t.stereo) or t.kind} | `{t.file.name}:{t.line}` | {t.loc} |" for t in sorted(ts, key=lambda x: x.name)]
        path = OUT / md_link_module(mod)
        path.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")
        written.append(path)
    return written


def module_graph(types, by_name):
    pairs = defaultdict(set)
    for t in types:
        for e in edges_of(t):
            if e in by_name and by_name[e].module != t.module:
                pairs[(t.module, by_name[e].module)].add((t.name, e))
    return pairs


def page_overview(types, by_name, modules, pairs):
    layer_pairs = defaultdict(int)
    for (a, b), s in pairs.items():
        la, lb = a.split("/")[0], b.split("/")[0]
        if la != lb:
            layer_pairs[(la, lb)] += len(s)
    lines = ["# Overview — layers & modules", "", "[← index](README.md)", "",
             "Intended direction: **Editor → UI → Gameplay → Core**. Red edges point the wrong way.", "",
             "## Layer dependencies", "", "Label = distinct type-to-type links.", "", "```mermaid", "flowchart TB"]
    layer_counts = defaultdict(int)
    for t in types:
        layer_counts[t.layer] += 1
    for layer in sorted(layer_counts, key=lambda l: -LAYER_RANK.get(l, -1)):
        lines.append(f"  {nid(layer)}[\"{layer}<br/>{layer_counts[layer]} types\"]")
    red = []
    for k, ((a, b), c) in enumerate(sorted(layer_pairs.items())):
        lines.append(f"  {nid(a)} -->|{c}| {nid(b)}")
        if wrong_way(a, b):
            red.append(k)
    if red:
        lines.append(f"  linkStyle {','.join(map(str, red))} stroke:#d33,stroke-width:2px,color:#d33")
    lines += ["```", "", "## Module dependencies", "",
              f"Box = folder module (types · LOC). Edge label = distinct type pairs; edges under {MIN_OVERVIEW_EDGE} pairs hidden.", "",
              "```mermaid", "flowchart LR"]
    by_layer = defaultdict(list)
    for mod in modules:
        by_layer[mod.split("/")[0]].append(mod)
    for layer in sorted(by_layer, key=lambda l: -LAYER_RANK.get(l, -1)):
        lines.append(f"  subgraph L_{nid(layer)}[\"{layer} layer\"]")
        for mod in sorted(by_layer[layer]):
            ts = modules[mod]
            loc = sum(t.loc for t in ts if not t.outer)
            lines.append(f"    M_{nid(mod)}[\"{mod}<br/>{len(ts)} types · {loc} LOC\"]")
        lines.append("  end")
    red, k = [], 0
    for (a, b), s in sorted(pairs.items()):
        if len(s) < MIN_OVERVIEW_EDGE:
            continue
        lines.append(f"  M_{nid(a)} -->|{len(s)}| M_{nid(b)}")
        if wrong_way(a.split("/")[0], b.split("/")[0]):
            red.append(k)
        k += 1
    if red:
        lines.append(f"  linkStyle {','.join(map(str, red))} stroke:#d33,stroke-width:2px")
    lines += ["```", "", "## Module table", "", "| Module | Types | LOC | Diagram |", "|---|---|---|---|"]
    for mod, ts in sorted(modules.items(), key=lambda x: -sum(t.loc for t in x[1] if not t.outer)):
        lines.append(f"| {mod} | {len(ts)} | {sum(t.loc for t in ts if not t.outer)} | [open]({md_link_module(mod)}) |")
    (OUT / "00-overview.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def page_inheritance(types, by_name):
    part = {t.name for t in types if t.base_class or t.interfaces}
    part |= {t.base_class for t in types if t.base_class} | {i for t in types for i in t.interfaces}
    lines = ["# Inheritance & interfaces", "", "[← index](README.md)", "",
             f"{len(part)} types in project-internal inheritance / implementation. Unity bases (MonoBehaviour, ScriptableObject) are stereotypes.", "",
             "```mermaid", "classDiagram", "  direction TB"]
    for name in sorted(part):
        t = by_name[name]
        lines += [f"  class {name} {{"] + [f"    <<{s}>>" for s in t.stereo[:2]] + ["  }"]
    for t in sorted(types, key=lambda x: x.name):
        if t.base_class:
            lines.append(f"  {t.base_class} <|-- {t.name}")
        for i in t.interfaces:
            lines.append(f"  {i} <|.. {t.name}")
    lines.append("```")
    (OUT / "01-inheritance.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def page_events(types, by_name):
    subs = [(t.name, p, e) for t in types for p, e in t.subs if p != t.name]
    by_pub = defaultdict(lambda: defaultdict(list))
    for s, p, e in subs:
        by_pub[p][e].append(s)
    lines = ["# Event flow", "", "[← index](README.md)", "",
             f"{len(subs)} `+=` subscriptions. Arrow = subscriber → publisher, label = event. UnityEvent/AddListener not included.", "",
             "```mermaid", "flowchart LR"]
    grouped = defaultdict(list)
    for s, p, e in subs:
        grouped[(s, p)].append(e)
    for (s, p), evs in sorted(grouped.items()):
        label = ", ".join(sorted(set(evs)))
        lines.append(f"  {nid(s)}[{s}] -->|{label}| {nid(p)}[{p}]")
    lines += ["```", "", "## Event catalogue", "", "| Publisher | Event | Subscribers |", "|---|---|---|"]
    for p in sorted(by_pub):
        for e in sorted(by_pub[p]):
            lines.append(f"| {p} | {e} | {', '.join(sorted(set(by_pub[p][e])))} |")
    declared = sorted((t.name, m.name) for t in types for m in t.members if m.kind == "event")
    unused = [f"{t}.{e}" for t, e in declared if e not in by_pub.get(t, {})]
    lines += ["", "## Declared events with no `+=` subscriber found", "",
              "May be wired via lambdas on another name, inspector, or genuinely unused.", "",
              ", ".join(unused) or "none"]
    (OUT / "02-events.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def page_metrics(types, by_name, files, pairs):
    top = [t for t in types if not t.outer and t.kind != "enum"]
    fan_in = defaultdict(set)
    for t in types:
        for e in edges_of(t):
            if e in by_name and e != t.name:
                fan_in[e].add(t.name)
    lines = ["# Metrics", "", "[← index](README.md)", "",
             "Generated. Coupling = distinct project types referenced (associations + usages).", "",
             "## Largest types", "", "| Type | LOC | Methods | Fields | Deps (fan-out) | Module |", "|---|---|---|---|---|---|"]
    for t in sorted(top, key=lambda x: -x.loc)[:20]:
        nm = sum(1 for m in t.members if m.kind == "method")
        nf = sum(1 for m in t.members if m.kind in ("field", "property"))
        lines.append(f"| {t.name} | {t.loc} | {nm} | {nf} | {len(edges_of(t) & set(by_name))} | {t.module} |")
    lines += ["", "## Most depended-on (fan-in)", "", "| Type | Used by | Examples |", "|---|---|---|"]
    for name, users in sorted(fan_in.items(), key=lambda x: -len(x[1]))[:15]:
        lines.append(f"| {name} | {len(users)} | {', '.join(sorted(users)[:5])} |")
    lines += ["", "## Most dependencies (fan-out)", "", "| Type | Depends on |", "|---|---|"]
    for t in sorted(types, key=lambda x: -len(edges_of(x) & set(by_name)))[:15]:
        lines.append(f"| {t.name} | {len(edges_of(t) & set(by_name))} |")

    singletons = sorted(t.name for t in types if any(m.name == "Instance" and m.static for m in t.members))
    lines += ["", f"## Static `Instance` singletons ({len(singletons)})", "", ", ".join(singletons)]

    def scan(pattern):
        hits = defaultdict(int)
        for f, (_, code) in files.items():
            c = len(re.findall(pattern, code))
            if c:
                hits[str(f.relative_to(SRC)).replace("\\", "/")] += c
        return hits
    finds = scan(r"\b(?:FindObjectOfType|FindFirstObjectByType|FindAnyObjectByType|FindObjectsByType|FindObjectsOfType|FindWithTag|FindGameObjectWithTag|FindGameObjectsWithTag)\b|GameObject\.Find\s*\(")
    logs = scan(r"\bDebug\.Log(?:Warning|Error)?\s*\(")
    updates = scan(r"\bvoid\s+(?:Update|FixedUpdate|LateUpdate)\s*\(")
    for title, hits in (("Scene searches (`Find*`)", finds), ("`Debug.Log*` calls", logs), ("Per-frame methods (`Update`/`FixedUpdate`/`LateUpdate`)", updates)):
        lines += ["", f"## {title} — {sum(hits.values())} in {len(hits)} files", "", "| File | Count |", "|---|---|"]
        lines += [f"| {f} | {c} |" for f, c in sorted(hits.items(), key=lambda x: -x[1])[:25]]

    viol = []
    for (a, b), s in sorted(pairs.items()):
        if wrong_way(a.split("/")[0], b.split("/")[0]):
            viol += [(a, b, x, y) for x, y in sorted(s)]
    lines += ["", f"## Layer violations — {len(viol)} type links", "", "Each row points the wrong way for Core ← Gameplay ← UI.", "",
              "| From module | To module | From type | To type |", "|---|---|---|---|"]
    lines += [f"| {a} | {b} | {x} | {y} |" for a, b, x, y in viol]

    adj = {t.name: {e for e in edges_of(t) if e in by_name} for t in types}
    sccs = tarjan(sorted(adj), adj)
    lines += ["", "## Dependency cycles (type level)", "", "Types that transitively depend on each other. Small cycles are cheapest to break.", "",
              "| Size | Types |", "|---|---|"]
    lines += [f"| {len(c)} | {', '.join(c) if len(c) <= 12 else ', '.join(c[:12]) + f' … +{len(c) - 12}'} |" for c in sccs]
    mods_in_big = sorted({by_name[n].module for n in sccs[0]}) if sccs else []
    lines += ["", f"Modules in the largest cycle ({len(mods_in_big)}): " + ", ".join(mods_in_big)]

    unref = sorted(t.name for t in top if not fan_in.get(t.name) and "MB" not in t.stereo and "SO" not in t.stereo
                   and "Editor" not in t.stereo and "Drawer" not in t.stereo and t.layer != "Editor")
    lines += ["", "## Plain types with no code reference", "",
              "Non-MonoBehaviour, non-ScriptableObject types nothing in code refers to. MB/SO usage is checked against scene/prefab GUIDs separately.", "",
              ", ".join(unref) or "none"]
    (OUT / "metrics.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")
    return sccs


def page_readme(types, modules, files):
    loc = sum(sum(1 for l in raw.splitlines() if l.strip()) for raw, _ in files.values())
    mb = sum(1 for t in types if "MB" in t.stereo)
    so = sum(1 for t in types if "SO" in t.stereo)
    lines = ["# Project Ocean — UML", "",
             f"Source: `Assets/Project_Ocean/Scripts` · {len(files)} files · {len(types)} types · {loc} non-blank lines · "
             f"{mb} MonoBehaviours · {so} ScriptableObjects · "
             f"{sum(1 for t in types if t.kind == 'interface')} interfaces · {sum(1 for t in types if t.kind == 'enum')} enums", "",
             "Regenerate after any script change: `python tools/uml/gen_uml.py` (self-test: `python tools/uml/test_gen_uml.py`).", "",
             "## Pages", "",
             "- [Overview — layers & modules](00-overview.md)",
             "- [Inheritance & interfaces](01-inheritance.md)",
             "- [Event flow](02-events.md)",
             "- [Metrics](metrics.md)",
             "- [Findings & refactor analysis](findings.md) (hand-written)", "",
             "## Modules", ""]
    for layer in sorted({m.split("/")[0] for m in modules}, key=lambda l: LAYER_RANK.get(l, 9)):
        lines.append(f"**{layer}**: " + " · ".join(f"[{m}]({md_link_module(m)})" for m in sorted(modules) if m.split("/")[0] == layer))
        lines.append("")
    lines += ["## Legend", "",
              "- `<|--` inherits · `<|..` implements · `-->` holds a field/property of that type (label = member)",
              "- `+` public · `#` protected · `$` static · `*` abstract · `~T~` = generic `<T>` · `Array` = `[]`",
              "- Stereotypes: `MB` MonoBehaviour · `SO` ScriptableObject · `static` · `abstract` · `interface` · `enum`",
              "- Class boxes show public/protected API only, capped at "
              f"{MAX_FIELDS} fields and {MAX_METHODS} methods. External types show their module as stereotype.", "",
              "## Limits", "",
              "Static text parser, not the compiler. Links made only via `GetComponent<T>`, inspector wiring, prefabs, "
              "`SendMessage` or strings are invisible; identical names can overcount. `InputSystem_Actions.cs` (generated) and `Scripts/Deprecated/` are excluded."]
    (OUT / "README.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def main():
    types, files = load()
    by_name, dupes = resolve(types)
    modules = defaultdict(list)
    for t in types:
        modules[t.module].append(t)
    OUT.mkdir(parents=True, exist_ok=True)
    for old in (OUT / "modules").glob("*.md") if (OUT / "modules").exists() else []:
        old.unlink()
    pairs = module_graph(types, by_name)
    page_modules(types, by_name, modules)
    page_overview(types, by_name, modules, pairs)
    page_inheritance(types, by_name)
    page_events(types, by_name)
    page_metrics(types, by_name, files, pairs)
    page_readme(types, modules, files)
    if dupes:
        print("warning: duplicate type names (first wins):", ", ".join(sorted(dupes)))
    print(f"{len(files)} files, {len(types)} types, {len(modules)} modules -> {OUT}")


if __name__ == "__main__":
    main()
