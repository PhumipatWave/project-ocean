# draw.io Project Structure Diagram — Implementation Plan

**Goal (user, 2026-09-29):** one draw.io file to read the whole project structure. Detail: **everything** (all fields and methods, private included).

**Approach:** a renderer on top of the existing parser (`tools/uml/gen_uml.py` already parses every type, member, base class, interface and association), so the diagram is regenerated from code and never drifts. New `tools/uml/gen_drawio.py`; `gen_uml.py`'s `main()` calls it. Output: `docs/uml/drawio/project-structure.drawio` (plain XML, opens in draw.io desktop, app.diagrams.net and the VS Code draw.io extension).

**Pages:**
1. **Overview:** the layers (UI → Gameplay → Core, plus Editor) as bands, each folder module as a box (types · LOC), module-to-module dependency arrows (≥ 3 type links, label = count, red = wrong-way), and a legend.
2. **One page per feature area**, each module as a container holding its UML class boxes:
   Core · Gameplay – Player & Entities · Gameplay – Enemies · Gameplay – Quests · Gameplay – Items, Spawning & Combat · Gameplay – Systems · UI · Editor.
   - Class box: stereotype (MonoBehaviour / ScriptableObject / interface / enum / static / abstract), name, then **all** fields/properties/events and **all** methods with visibility (`+ - # ~`). Enums list their values. Colour by stereotype (legend on Overview).
   - Arrows: inheritance (solid, hollow triangle), implements (dashed, hollow triangle), field/property association (open arrow, labelled with the field name). A base class or interface from another page appears as a small grey "external" box.
   - Plain "uses" dependencies are left off (hundreds of them); the Overview arrows summarise them per module.
   - Layout is computed (draw.io doesn't auto-layout on open): classes packed into balanced columns per module, modules packed in rows.

**Checks:** unit test (XML parses; every sample type becomes a cell; inheritance edge present); the generated file contains all 270 types; existing 9 generator tests still pass; open-check by parsing with ElementTree and a screenshot-free sanity count per page.
