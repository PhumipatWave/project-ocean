# Cleanup Phase 6c: Find* Calls — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the fragile `Find*` lookups and keep the ones that are the right tool, each with a stated reason.

**Survey (2026-09-29):** 11 live calls (the roadmap's 14 included `Deprecated/`/`Editor/`). None runs per frame; all run once in `Awake`/`OnEnable` or on scene load, so there's no performance problem. The problem is fragility.

| Call | Verdict |
|---|---|
| `MoveKeyBindUI` ×2: `FindFirstObjectByType<KeyBindManager>()` | **Replace** with `KeyBindManager.Instance` (the singleton already exists). |
| `Player` ×3 and `GameManager` ×1: `FindGameObjectWithTag("PlayerSpawnPoint" / "OxygenTubePoint")` | **Keep the tag lookup.** It's how a persistent object finds a point in a freshly loaded scene, and a serialized reference can't cross scenes. A registry component would need scene edits for no real gain. **Replace the 4 string literals** with `Tags.PlayerSpawnPoint` / `Tags.OxygenTubePoint` constants in `Core/Tags.cs`, next to `SceneNames`, so a typo is a compile error. |
| `ShopManager.FindTMP("AMM_WP1..4")`: `GameObject.Find` by object name | **Broken and dead** (verified live 2026-09-29). All 4 texts resolve to NULL: they're on the HUD's `WeaponUIPanel` (in `AS_PlayerHUD`), which is inactive on Land, and `GameObject.Find` skips inactive objects. The feature it serves is also unused: `Ammo1`–`Ammo4` (ItemType Ammo) are referenced by no shop list, scene, prefab or asset, so ammo can't be bought. **User decides** (see below). |
| `NPCManager.Awake`: `FindObjectsByType<NPC>` | **Keep.** Documented fix for an Awake-order race (NPCs that tried to register before `NPCManager.Instance` existed). One-time. |
| `UIManager.RefreshRegisteredWindows`: `FindObjectsByType<UIWindow>` | **Keep.** Fallback when a window isn't registered yet after an additive scene load. |
| `PlayerCamera.OnSceneLoaded`, `DoorInteract.Awake`: `FindFirstObjectByType<CinemachineCamera>` | **Keep.** A persistent script finding the camera of the loaded scene; one-time; same cross-scene reason as the tags. |

**Noted, not changed:** `Player.OnSceneLoaded` and `GameManager`'s scene-loaded handler both move the player to `PlayerSpawnPoint`. It's duplicated responsibility, but merging them means reasoning about handler order across scene loads, which is a behaviour risk outside this cleanup.

**Ammo HUD options (user):**
- **A. Remove** the dead code: `ammoTexts`, `FindTMP`, the HUD update loop in `UpdateUI`. Keep `ammoCounts`, which `ShopTransactionService.BuyItem` still takes.
- **B. Disable** with `#if ENABLE_AMMO_SHOP`, like the checkpoint item.
- **C. Fix properly:** `ShopManager` raises `OnAmmoCountsChanged(int[])`, and a small view component on `WeaponUIPanel.prefab` (with the 4 texts serialized) updates them. That's a prefab edit, and only worth it if ammo will be sold.

## Global Constraints

- No behaviour change except where it fixes the broken lookup (and there's nothing to see, since ammo can't be bought).
- Files keep their encoding, BOM and line endings. No scene/prefab edits unless option C is chosen.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.

---

### Task 1: Code changes
- [ ] `Core/Tags.cs` with the two constants; replace the 4 literals.
- [ ] `MoveKeyBindUI`: both lookups → `KeyBindManager.Instance`.
- [ ] Ammo HUD per the user's option.
- [ ] Compile 0 errors; tests all pass.

### Task 2: Verify live
- [ ] Load slot 0 → Land (player at the spawn point) → Ocean (player at the spawn point, rope start at `OxygenTubePoint`) → Land. Open the key-bind settings: the Move rebind UI shows the current keys. Console: no new errors. Restore saves, no scene/prefab changes.

### Task 3: Docs
- [ ] `findings.md` Phase 6: mark the Find item done with the table's verdicts; note the ammo finding. UML regen + tests, `graphify update .`. Hand-off, suggested commit "chore: tag constants, KeyBindManager.Instance, drop broken ammo-HUD lookup". Do not commit.
