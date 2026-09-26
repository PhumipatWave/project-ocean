# UML refresh & refactor roadmap: design

Date: 2026-09-26 · Branch: `develop-26sep26-refactor`

## Goal

Replace the stale UML PDFs (2026-09-19 snapshot) with a regenerable map of `Assets/Project_Ocean/Scripts`, move unused scripts aside, and produce a phased refactor plan.

## Decisions (agreed with the user)

| Question | Decision |
|---|---|
| UML format | Mermaid `.md` in `docs/uml/` |
| Class box detail | Public/protected API + relations; private members hidden |
| Generation | `tools/uml/gen_uml.py` (Python stdlib regex parser), re-run after each refactor phase |
| Refactor priority | Structure first: layering, folder names, asmdefs; class splits later |
| Namespaces | Rename `Game.Testing` → `Game.Core` / `Game.Gameplay` / `Game.UI` with matching asmdefs |
| Old PDFs | Deleted |
| Unused scripts | Moved to `Scripts/Deprecated/` with `.meta`; recently edited ones kept and flagged |
| Git | Claude does not commit or push; the user does |

## Deliverables

- `tools/uml/gen_uml.py` and self-test `tools/uml/test_gen_uml.py`.
- `docs/uml/`: `README.md`, `00-overview.md`, `01-inheritance.md`, `02-events.md`, `metrics.md`, `modules/*.md` (generated), `findings.md` (hand-written analysis + roadmap).
- `Assets/Project_Ocean/Scripts/Deprecated/` (10 scripts).

## Verification

- Parser self-test passes.
- All Mermaid blocks parse with mermaid@11, checked by a scratch script.
- Unity recompiled after the moves with no `CS` errors.

## Next

The refactor itself follows `docs/uml/findings.md` §4, one phase per spec/plan/branch, starting with Phase 1 (names).
