# Refactor Phase 10: Break the 36-Type Gameplay Cycle — Implementation Plan

**Goal:** Remove the largest dependency cycle (36 types across 16 modules: entities, interaction, items, projectiles, quests, tutorial, cutscenes, services). After this phase the remaining cycles stay inside one feature each (enemy FSM, player core, NPC/quest navigation, enemy core), which is normal.

**Status:** done 2026-09-30; the regenerated cycle table matches the "after 10d" row exactly.

**How the cuts were chosen:** `tools/uml/gen_uml.py`'s type graph + Tarjan SCC, simulated edge removal (2026-09-30). Two other candidates (FeatureUnlockService → QuestManager, UpgradeService → PlayerRegistry) did not shrink the cycle and are left alone.

| Step | Cut | Largest cycle after |
|---|---|---|
| start | — | 36 |
| 10a | `EntityDeathHandler` → `Boid`, `EnemyNavAgentBase`, `IEnemyBrain` | 15 |
| 10b | `TutorialGate` moves to Core; drops `TutorialPromptUI` | 12 |
| 10c | `PlayerInteractor` → `PlayerSelfDialogueUI` | 12 (splits the interaction/NPC group off) |
| 10d | `QuestStepEffects` → `PlayerRegistry` | **11** (enemy FSM only) |

Remaining after 10d: enemy FSM 11 (profiles ↔ states, by design), player core 8 (Player ↔ gun ↔ bullet ↔ item use), NPC/quest navigation 7, enemy core 6.

## Global constraints

- No behaviour change. Save format unchanged. No scene edits expected (static classes and code only); prefab edits none.
- Files keep encoding/line endings. CLAUDE.md comment rules. The user commits; moving a file uses a plain filesystem move of `.cs` + `.meta` (keeps the GUID), never `git mv`.
- After each step: compile clean, EditMode all pass (baseline 63), `python tools/uml/gen_uml.py` and check the cycle table matches the row above.
- Back up the save folder before any Play test; verify byte-identical after.

---

### 10a: `EntityDeathHandler` stops knowing enemies (was deferred 9c)
- `EntityDeathHandler`: remove `navAgent`, `StopAllNavigation`, `DisableNavAgent`, `HandleDeath(IEnemyBrain)`, the NavMeshAgent/nav restore in `OnGetFromPool`; remember the tag it started with in `Awake` and restore that (not the hard-coded `"Enemy"`).
- `EnemyNavAgentBase`: new `StopForDeath()` (stop + reset the NavMeshAgent, disable itself and its `Boid`); its existing `OnGetFromPool` re-enables the agent (move the restore lines from the death handler).
- `Enemy.OnDeathHandler`: `brain?.OnDeath(); navAgentHost?.StopForDeath(); entityDeathHandler?.HandleDeath();`
- Live: kill a fish → stops, red, tag `Items`; return it to the pool and respawn → alive, white, tag `Enemy`, moving.

### 10b: `TutorialGate` becomes a Core type
- Move `Gameplay/QuestSystems/Quest/TutorialGate.cs` (+ `.meta`) to `Core/Events/TutorialGate.cs`, namespace `Game.Core`. It only needs `TutorialActionId` and `NotificationManager` (both Core).
- Add `public static string CurrentPrompt` to `TutorialGate`; `TutorialPromptUI` sets it where it sets its own `CurrentPrompt` today, and `NotifyBlocked` reads it. `TutorialPromptUI.CurrentPrompt` stays as a forwarding property only if something else reads it.
- `TutorialStep` (reads QuestManager) stays in the quest folder; its callers are UI (shop, upgrade slot, HUD) and the prompt, which is the legal direction.
- Callers (`Player`, `Interactable`, `GameManager`, `GameHUD`, `TutorialPromptUI`) already use `Game.Core`; only missing `using` lines change.

### 10c: the self-dialogue "blocks interaction" flag moves to Core
- `DialogueState` (Core) gets `BlocksInteraction` (showing a self-dialogue, or hidden this frame). `PlayerSelfDialogueUI` sets it where it sets `showing`/`lastHideFrame` today and keeps its own property as a forward.
- `PlayerInteractor` (and `Interactable` if it reads it) use `DialogueState.BlocksInteraction`.

### 10d: quest money checks stop going through `PlayerRegistry`
- `PlayerMoney.Current` (static, registered in `Awake`, cleared in `OnDestroy`), the same pattern as `Inventory.Current`.
- `QuestStepEffects.MoneyRequirementMet` and the delivery money charge use `PlayerMoney.Current`.
- `UpgradeService` may switch to `PlayerMoney.Current` for the price too (not needed for the cycle).

### Verify (end)
- Cycle table: largest 11 (enemy FSM), no cycle spans more than one feature.
- Live: kill + pool reuse (10a), tutorial prompt blocked-press toast and E during a Move step (10b), E right after a self-dialogue closes does nothing (10c), a delivery quest with a money requirement (10d) or `MoneyRequirementMet` via `execute_code`.
- `findings.md` Phase 10 entry, UML regen, `graphify update .`.

## Not in this phase
- Moving the Move-tutorial edge detection out of `Player.ApplyMovement` (15 lines): it reads aim/lock state that only Player has; do it with the Player split (Phase 11 candidate).
- `SaveBridge` participants, `InventoryManager` drag controller, `UIManager` review: separate phases.
