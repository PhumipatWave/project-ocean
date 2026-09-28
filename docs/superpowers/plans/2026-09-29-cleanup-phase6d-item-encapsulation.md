# Cleanup Phase 6d: Encapsulate Item Fields — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `Item` (ScriptableObject) data can't be changed from code by accident. In the editor, a runtime write to a ScriptableObject field silently edits the asset on disk. Asset data must stay byte-for-byte the same.

**Survey (2026-09-29):**
- Runtime writes to `Item` or `EnemyBaseProfile` fields: **none**. The only `Item` writes are in `PlayerSplitTests`; the "profile" hits are writes to `NavMeshAgent.speed` and to local fields. No editor script reads these fields by name (`FindProperty`).
- `Item`: 10 public fields, 24 assets, 60 reads in 12 files.
- `EnemyBaseProfile`: 21 public camelCase fields (+ 4 subclasses), 5 assets, 55 reads.

**Scope decision:**
- **`Item`: do it.** Each field becomes `[SerializeField, FormerlySerializedAs("<OldName>")] private <T> <camelName>;` with a read-only property of the **same old name** (`public int ItemID => itemID;`). So none of the 60 call sites change, and the attributes (`Header`, `TextArea`, `Range`) move onto the private fields.
- **`EnemyBaseProfile`: skip.** Its fields are camelCase, so properties would need new PascalCase names and all 55 call sites would change, plus the subclasses and their assets. That's a lot of churn and asset risk to guard against writes nobody makes. Revisit if runtime tuning code ever appears.

**Why `FormerlySerializedAs` is enough:** Unity reads the old YAML key (`ItemID:`) into the renamed field. The asset files aren't re-saved by this change, so git shows no `.asset` diff. When an asset is next saved from the editor, it writes the new key. The attribute must stay in the code from then on.

## Global Constraints

- Zero asset data change: a before/after value snapshot of all 24 `Item` assets must match exactly, and `git status` must show no `.asset` change.
- Public read API unchanged (same property names and types).
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.

---

### Task 1: Snapshot
- [ ] `execute_code`: for every `Item` asset under `Assets/Project_Ocean`, record the path + the 10 values (object refs as asset GUIDs + local file id) into `$TEMP/p6d_items_before.json`.

### Task 2: Encapsulate
- [ ] `Item.cs`: 10 fields → private serialized fields with `FormerlySerializedAs`, same-name read-only properties.
- [ ] `PlayerSplitTests.BuildingItem`: set the 3 values through `SerializedObject` (test-only), not public setters.
- [ ] Compile 0 errors (any external write that the survey missed shows up here as `CS0200`); tests all pass.

### Task 3: Verify
- [ ] Same snapshot → `$TEMP/p6d_items_after.json`; diff: identical.
- [ ] `git status --short -- "*.asset"`: no Item asset changed.
- [ ] Play smoke: load slot 0 → Land: shop shows item names/prices; buy and sell one item; inventory shows icons and stack counts. → Ocean: use the MiniHub (ItemPrefab path). Console: no new errors. Restore saves.
- [ ] `findings.md` Phase 6: mark done (Item) and skipped with reason (EnemyBaseProfile); Phase 6 complete. UML regen + tests, `graphify update .`. Hand-off, suggested commit "refactor(item): read-only Item fields (FormerlySerializedAs keeps asset data)". Do not commit.
