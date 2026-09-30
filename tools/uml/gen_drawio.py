"""Render gen_uml's parsed model as a multi-page draw.io file (docs/uml/drawio/project-structure.drawio).

Called from gen_uml.main(); run alone with: python tools/uml/gen_drawio.py
"""
import html
import math
from collections import defaultdict

import gen_uml as g

OUT_FILE = g.OUT / "drawio" / "project-structure.drawio"

STEREO_NAME = {"MB": "MonoBehaviour", "SO": "ScriptableObject"}
# (fill, stroke) by stereotype; first match wins.
COLORS = [("interface", "#e1d5e7", "#9673a6"), ("enum", "#fff2cc", "#d6b656"), ("SO", "#d5e8d4", "#82b366"),
          ("MB", "#dae8fc", "#6c8ebf"), ("static", "#f5f5f5", "#666666"), ("struct", "#ffe6cc", "#d79b00")]
PLAIN = ("#ffffff", "#333333")
LAYER_FILL = {"UI": "#fdf0e6", "Gameplay": "#eef5fb", "Core": "#eef8ee", "Editor": "#f3f3f3"}

LINE_H, HEAD_H, CHAR_W = 15, 40, 6.4
MIN_W, MAX_W, MAX_TYPE = 170, 700, 44
COL_GAP, BOX_GAP, CONT_PAD, CONT_HEAD = 40, 30, 20, 30
ROW_LIMIT, CONT_GAP = 4200, 80
ALL_ROW_LIMIT = 9000

PAGES = ["Core", "Gameplay – Player & Entities", "Gameplay – Enemies", "Gameplay – Quests",
         "Gameplay – Items, Spawning & Combat", "Gameplay – Systems", "UI", "Editor"]


def page_of(module):
    parts = module.split("/")
    if parts[0] != "Gameplay":
        return parts[0] if parts[0] in PAGES else "Core"
    sub = parts[1] if len(parts) > 1 else ""
    if sub == "Entities":
        return "Gameplay – Enemies" if len(parts) > 2 and parts[2] == "Enemies" else "Gameplay – Player & Entities"
    if sub == "QuestSystems":
        return "Gameplay – Quests"
    if sub in ("Items", "Inventory", "Spawning", "Projectiles"):
        return "Gameplay – Items, Spawning & Combat"
    return "Gameplay – Systems"


def colors_of(t):
    for key, fill, stroke in COLORS:
        if key in t.stereo:
            return fill, stroke
    return PLAIN


def short(s):
    s = " ".join(s.split())
    return s if len(s) <= MAX_TYPE else s[:MAX_TYPE - 1] + "…"


def member_line(m):
    vis = g.VIS[m.vis]
    if m.kind in ("method", "ctor"):
        text = f"{vis} {m.name}({short(m.params)})" + ("" if m.kind == "ctor" else f": {short(m.type)}")
    else:
        text = f"{vis} {'event ' if m.kind == 'event' else ''}{m.name}: {short(m.type)}"
    shown = html.escape(text)
    if m.static:
        shown = f"<u>{shown}</u>"
    if m.abstract:
        shown = f"<i>{shown}</i>"
    return text, shown


def class_box(t):
    """(html label, width, height) for one UML class box."""
    stereo = [STEREO_NAME.get(s, s) for s in t.stereo[:2]]
    title = t.name + (f" (in {t.outer})" if t.outer else "")
    rows_plain = [title] + [f"«{s}»" for s in stereo]
    parts = [f"<div style='text-align:center'>" + "".join(f"<i>«{html.escape(s)}»</i><br>" for s in stereo)
             + f"<b>{html.escape(title)}</b></div>"]
    sections = []
    if t.kind == "enum":
        sections.append([(v, html.escape(v)) for v in t.enum_values])
    else:
        data = [m for m in t.members if m.kind in ("field", "property", "event")]
        funcs = [m for m in t.members if m.kind in ("method", "ctor")]
        sections += [[member_line(m) for m in data], [member_line(m) for m in funcs]]
    lines = 0
    for sec in sections:
        parts.append("<hr style='margin:2px 0'>" + ("<br>".join(s for _, s in sec) if sec else "&nbsp;"))
        rows_plain += [p for p, _ in sec]
        lines += max(1, len(sec))
    width = int(min(MAX_W, max(MIN_W, CHAR_W * max(len(r) for r in rows_plain) + 20)))
    height = HEAD_H + 12 * len(stereo) + LINE_H * lines + 8 * len(sections)
    return "".join(parts), width, height


class Page:
    def __init__(self, name, idx):
        self.name, self.idx, self.cells, self.n = name, idx, [], 0

    def new_id(self, hint):
        self.n += 1
        return f"p{self.idx}-{self.n}-{g.nid(hint)[:40]}"

    def vertex(self, hint, value, style, x, y, w, h, parent="1"):
        cid = self.new_id(hint)
        self.cells.append(f'<mxCell id="{cid}" value="{html.escape(value, quote=True)}" style="{style}" vertex="1" '
                          f'parent="{parent}"><mxGeometry x="{x:.0f}" y="{y:.0f}" width="{w:.0f}" height="{h:.0f}" '
                          f'as="geometry"/></mxCell>')
        return cid

    def edge(self, src, dst, style, label=""):
        cid = self.new_id("e")
        self.cells.append(f'<mxCell id="{cid}" value="{html.escape(label, quote=True)}" style="{style}" edge="1" '
                          f'parent="1" source="{src}" target="{dst}"><mxGeometry relative="1" as="geometry"/></mxCell>')

    def xml(self):
        body = '<mxCell id="0"/><mxCell id="1" parent="0"/>' + "".join(self.cells)
        return (f'<diagram name="{html.escape(self.name, quote=True)}" id="page{self.idx}">'
                f'<mxGraphModel grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" '
                f'page="0" pageScale="1" math="0" shadow="0"><root>{body}</root></mxGraphModel></diagram>')


def pack_rows(items, limit):
    """items: [(key, w, h)] -> {key: (x, y)} left-to-right rows wrapping at `limit`."""
    pos, x, y, row_h = {}, 0, 0, 0
    for key, w, h in items:
        if x and x + w > limit:
            x, y, row_h = 0, y + row_h + CONT_GAP, 0
        pos[key] = (x, y)
        x += w + CONT_GAP
        row_h = max(row_h, h)
    return pos


CLASS_STYLE = ("rounded=0;whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacingLeft=6;spacingTop=2;"
               "fontSize=11;fontFamily=Helvetica;fillColor={fill};strokeColor={stroke};")
CONT_STYLE = ("swimlane;startSize=30;html=1;whiteSpace=wrap;collapsible=0;fontStyle=1;fontSize=13;align=left;"
              "spacingLeft=8;fillColor={fill};swimlaneFillColor={fill};strokeColor=#9e9e9e;")
INHERIT = "endArrow=block;endFill=0;endSize=14;html=1;edgeStyle=orthogonalEdgeStyle;rounded=1;strokeColor=#333333;"
IMPLEMENT = INHERIT + "dashed=1;strokeColor=#9673a6;"
ASSOC = ("endArrow=open;endSize=10;html=1;edgeStyle=orthogonalEdgeStyle;rounded=1;strokeColor=#6c8ebf;"
         "fontSize=10;fontColor=#3d5a80;labelBackgroundColor=#ffffff;")


def module_container(page, mod, ts, x, y, cell_ids):
    """Place `ts` (already measured) in balanced columns inside a module container at (x, y)."""
    boxes = sorted(((t,) + class_box(t) for t in ts), key=lambda b: -b[3])
    ncols = 1 if len(boxes) == 1 else min(5, max(2, math.ceil(math.sqrt(len(boxes)))))
    cols = [[] for _ in range(ncols)]
    heights = [0] * ncols
    for b in boxes:
        c = heights.index(min(heights))
        cols[c].append(b)
        heights[c] += b[3] + BOX_GAP
    widths = [max((b[2] for b in col), default=0) for col in cols]
    w = CONT_PAD * 2 + sum(widths) + COL_GAP * (ncols - 1)
    h = CONT_HEAD + CONT_PAD + max(heights)
    fill = LAYER_FILL.get(mod.split("/")[0], "#f7f7f7")
    cont = page.vertex(mod, f"{mod}   ({len(ts)} types)", CONT_STYLE.format(fill=fill), x, y, w, h)
    cx = CONT_PAD
    for col, cw in zip(cols, widths):
        cy = CONT_HEAD + CONT_PAD / 2
        for t, label, bw, bh in col:
            fill_c, stroke = colors_of(t)
            cell_ids[t.name] = page.vertex(t.name, label, CLASS_STYLE.format(fill=fill_c, stroke=stroke),
                                           cx, cy, cw, bh, parent=cont)
            cy += bh + BOX_GAP
        cx += cw + COL_GAP
    return w, h


def measure_container(ts):
    boxes = sorted((class_box(t) for t in ts), key=lambda b: -b[2])
    ncols = 1 if len(boxes) == 1 else min(5, max(2, math.ceil(math.sqrt(len(boxes)))))
    heights, widths = [0] * ncols, [0] * ncols
    for _, bw, bh in boxes:
        c = heights.index(min(heights))
        heights[c] += bh + BOX_GAP
        widths[c] = max(widths[c], bw)
    return CONT_PAD * 2 + sum(widths) + COL_GAP * (ncols - 1), CONT_HEAD + CONT_PAD + max(heights)


def feature_page(idx, name, mods, by_name, row_limit=ROW_LIMIT):
    page = Page(name, idx)
    sizes = [(m, *measure_container(ts)) for m, ts in sorted(mods.items())]
    pos = pack_rows(sizes, row_limit)
    cell_ids = {}
    bottom = 0
    for m, w, h in sizes:
        x, y = pos[m]
        module_container(page, m, mods[m], x, y, cell_ids)
        bottom = max(bottom, y + h)
    # Bases/interfaces living on another page get a small grey stand-in so the arrow has a target.
    external = sorted({n for ts in mods.values() for t in ts for n in ([t.base_class] if t.base_class else []) + t.interfaces
                       if n not in cell_ids and n in by_name})
    if external:
        ext_w = 260
        cols = max(1, min(8, row_limit // (ext_w + 30)))
        rows = math.ceil(len(external) / cols)
        cont = page.vertex("external", "External (defined on another page)",
                           CONT_STYLE.format(fill="#fafafa"), 0, bottom + CONT_GAP,
                           CONT_PAD * 2 + cols * (ext_w + 30), CONT_HEAD + CONT_PAD + rows * 60)
        for i, n in enumerate(external):
            t = by_name[n]
            cell_ids[n] = page.vertex(n, f"<b>{html.escape(n)}</b><br><font color='#777777'>{html.escape(t.module)}</font>",
                                      "rounded=1;whiteSpace=wrap;html=1;fontSize=11;fillColor=#eeeeee;strokeColor=#999999;dashed=1;",
                                      CONT_PAD + (i % cols) * (ext_w + 30), CONT_HEAD + (i // cols) * 60 + 10,
                                      ext_w, 44, parent=cont)
    for ts in mods.values():
        for t in ts:
            src = cell_ids[t.name]
            if t.base_class in cell_ids:
                page.edge(src, cell_ids[t.base_class], INHERIT)
            for i in t.interfaces:
                if i in cell_ids:
                    page.edge(src, cell_ids[i], IMPLEMENT)
            for target, member in sorted(t.assoc.items()):
                if target in cell_ids and target != t.base_class:
                    page.edge(src, cell_ids[target], ASSOC, member)
    return page


def overview_page(types, modules, pairs):
    page = Page("Overview", 0)
    layers = ["Editor", "UI", "Gameplay", "Core"]
    by_layer = defaultdict(list)
    for m in modules:
        by_layer[m.split("/")[0]].append(m)
    box_w, box_h, gap, per_row = 250, 50, 24, 12
    y, ids = 0, {}
    page.vertex("title", "<b style='font-size:20px'>Project Ocean — project structure</b><br>"
                "Arrows: module → module it uses (≥ %d type links, label = count). "
                "<font color='#d33'>Red</font> = points against Editor → UI → Gameplay → Core." % g.MIN_OVERVIEW_EDGE,
                "text;html=1;align=left;verticalAlign=top;whiteSpace=wrap;fontSize=13;", 0, -110, 1400, 80)
    for layer in layers:
        mods = sorted(by_layer.get(layer, []))
        if not mods:
            continue
        rows = math.ceil(len(mods) / per_row)
        w = CONT_PAD * 2 + per_row * (box_w + gap)
        h = CONT_HEAD + CONT_PAD * 2 + rows * (box_h + gap)
        n_types = sum(len(modules[m]) for m in mods)
        cont = page.vertex(layer, f"{layer} layer   ({n_types} types)",
                           CONT_STYLE.format(fill=LAYER_FILL.get(layer, "#f7f7f7")), 0, y, w, h)
        for i, m in enumerate(mods):
            loc = sum(t.loc for t in modules[m] if not t.outer)
            ids[m] = page.vertex(m, f"<b>{html.escape(m)}</b><br>{len(modules[m])} types · {loc} LOC · "
                                    f"<i>page: {html.escape(page_of(m))}</i>",
                                 "rounded=1;whiteSpace=wrap;html=1;fontSize=10;fillColor=#ffffff;strokeColor=#666666;",
                                 CONT_PAD + (i % per_row) * (box_w + gap), CONT_HEAD + CONT_PAD + (i // per_row) * (box_h + gap),
                                 box_w, box_h, parent=cont)
        y += h + 120
    for (a, b), s in sorted(pairs.items()):
        if len(s) < g.MIN_OVERVIEW_EDGE or a not in ids or b not in ids:
            continue
        red = g.wrong_way(a.split("/")[0], b.split("/")[0])
        page.edge(ids[a], ids[b], "endArrow=open;html=1;rounded=1;fontSize=10;opacity=60;"
                  + ("strokeColor=#dd3333;strokeWidth=2;" if red else "strokeColor=#7a7a7a;"), str(len(s)))
    # legend
    lx = CONT_PAD * 2 + per_row * (box_w + gap) + 80
    legend = [("MonoBehaviour", "#dae8fc", "#6c8ebf"), ("ScriptableObject", "#d5e8d4", "#82b366"),
              ("interface", "#e1d5e7", "#9673a6"), ("enum", "#fff2cc", "#d6b656"),
              ("static class", "#f5f5f5", "#666666"), ("struct", "#ffe6cc", "#d79b00"), ("plain C# class", *PLAIN)]
    cont = page.vertex("legend", "Legend", CONT_STYLE.format(fill="#ffffff"), lx, 0, 330, 60 + 34 * (len(legend) + 5))
    for i, (label, fill, stroke) in enumerate(legend):
        page.vertex(label, label, f"rounded=0;html=1;fontSize=11;fillColor={fill};strokeColor={stroke};",
                    16, 44 + i * 34, 150, 26, parent=cont)
    notes = ["+ public  - private  # protected  ~ internal", "<u>underlined</u> = static, <i>italic</i> = abstract",
             "solid ▷ = inherits, dashed ▷ = implements", "open → = field/property of that type (label = field)",
             "grey dashed box = type from another page"]
    for i, n in enumerate(notes):
        page.vertex("note", n, "text;html=1;fontSize=11;align=left;", 16, 44 + (len(legend) + i) * 34, 300, 26, parent=cont)
    return page


def build(types, by_name, modules, pairs):
    grouped = defaultdict(dict)
    for m, ts in modules.items():
        grouped[page_of(m)][m] = ts
    # Every module on one page, wider rows since it holds the whole project.
    pages = [overview_page(types, modules, pairs),
             feature_page(len(PAGES) + 1, "All Scripts", dict(modules), by_name, row_limit=ALL_ROW_LIMIT)]
    for i, name in enumerate(PAGES, 1):
        if grouped.get(name):
            pages.append(feature_page(i, name, grouped[name], by_name))
    return ('<mxfile host="gen_drawio" agent="tools/uml/gen_drawio.py" version="24.0.0">'
            + "".join(p.xml() for p in pages) + "</mxfile>\n")


def write(types, by_name, modules, pairs):
    OUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    OUT_FILE.write_text(build(types, by_name, modules, pairs), encoding="utf-8", newline="\n")
    return OUT_FILE


if __name__ == "__main__":
    types, _ = g.load()
    by_name, _ = g.resolve(types)
    modules = defaultdict(list)
    for t in types:
        modules[t.module].append(t)
    print("wrote", write(types, by_name, modules, g.module_graph(types, by_name)))
