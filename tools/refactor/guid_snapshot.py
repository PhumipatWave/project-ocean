"""Snapshot and compare .meta GUIDs under Assets/Project_Ocean/Scripts.

Moves must keep GUIDs, or scene/prefab links break. Usage:
  python tools/refactor/guid_snapshot.py snapshot before.json
  python tools/refactor/guid_snapshot.py compare before.json after.json
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCRIPTS = ROOT / "Assets" / "Project_Ocean" / "Scripts"


def snapshot():
    out = {}
    for meta in SCRIPTS.rglob("*.meta"):
        m = re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8", errors="replace"), re.M)
        if m:
            out[m.group(1)] = meta.relative_to(ROOT).as_posix()[:-5]
    return out


def compare(before, after):
    missing = {g: p for g, p in before.items() if g not in after}
    moved = {g: (p, after[g]) for g, p in before.items() if g in after and after[g] != p}
    for g, (a, b) in sorted(moved.items(), key=lambda x: x[1][0]):
        print(f"moved   {a} -> {b}")
    for g, p in sorted(missing.items(), key=lambda x: x[1]):
        print(f"MISSING {p} ({g})")
    print(f"{len(moved)} moved, {len(missing)} missing, {len(after) - len(before) + len(missing)} new")
    return 1 if missing else 0


if __name__ == "__main__":
    if sys.argv[1] == "snapshot":
        Path(sys.argv[2]).write_text(json.dumps(snapshot(), indent=1, sort_keys=True), encoding="utf-8")
        print(f"wrote {sys.argv[2]}")
    else:
        load = lambda p: json.loads(Path(p).read_text(encoding="utf-8"))
        sys.exit(compare(load(sys.argv[2]), load(sys.argv[3])))
