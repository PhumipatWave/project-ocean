# Refactor Phase 5a: Split QuestManager — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `QuestManager` (955 lines, 47 methods) keeps one job: the quest registry and lifecycle (accept, reject, cancel, turn in, tracking, the step-completion pipeline). Step evaluation, step side effects and save mapping move to their own classes. Behaviour is identical and the public API is unchanged, so no caller changes.

**Architecture:** all new files go next to `QuestManager.cs` in `Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/`, namespace `Game.Gameplay`, and are plain C# (no MonoBehaviour, so no scene or prefab changes).

| New class | Moved from QuestManager (verbatim bodies) | Size |
|---|---|---|
| `QuestProgressTracker` (instance) | `HandleEnemyKilled`, `HandleMoneyChanged`, `HandleDayOrDiveChanged` → `public CheckDayOrDive()`, `HandleTutorialAction`, `HandleNpcTalk`, `HandleAreaReached`, `HandleCutsceneFinished`, `HandleInventoryChanged` → `public CheckInventory()`, `HeldCountOf`, `TryProgress`, and the 9 event subscriptions (`Subscribe()` / `Unsubscribe()`) | ~260 |
| `QuestStepEffects` (static) | `MoneyRequirementMet`, `ApplyStepFeatureChanges` → `ApplyFeatureChanges`, `RemoveQuestItems`, `StampWaitingQuestStart` → `StampWaitingStart` | ~110 |
| `QuestSaveMapper` (static) | the field mapping of `GetSaveState` → `ToSaveData(IEnumerable<Quest>)` and of `LoadSaveState` → `Apply(List<QuestSaveData>, Func<int, Quest>)` | ~45 |

`QuestManager` keeps (~480 lines): `Instance`, the three static events, `Awake`/`OnEnable`/`OnDisable`, `ResetToDefault`, `InitializeQuests`, `EnsureQuestTracked`, `CompleteAutoDialogue`, `CompleteProgress`, the step-description toast, `HandleNextQuest`, all queries, `GetSaveState`/`LoadSaveState` (now thin wrappers), `ISaveParticipant`, accept/reject/cancel/turn-in, tracking and `DevSkipCurrentStep`.

**Wiring:**
```csharp
// QuestManager
private QuestProgressTracker progress;
private void Awake() { ...; InitializeQuests(); progress = new QuestProgressTracker(activeQuestList, CompleteProgress, RaiseQuestUpdated); }
private void OnEnable()  => progress?.Subscribe();   // null on a duplicate destroyed in Awake
private void OnDisable() => progress?.Unsubscribe();
private static void RaiseQuestUpdated() => OnQuestUpdated?.Invoke();
```
```csharp
// QuestProgressTracker
public QuestProgressTracker(List<Quest> quests, Action<Quest, int> completeProgress, Action raiseUpdated)
```
The tracker holds the same `List<Quest>` instance (`InitializeQuests` only clears and refills it, never replaces it), so `ResetToDefault` needs no re-wiring. Inside moved bodies: `CompleteProgress(q)` → `completeProgress(q, 1)`, `CompleteProgress(q, n)` → `completeProgress(q, n)`, `OnQuestUpdated?.Invoke()` → `raiseUpdated()`, `activeQuestList` → `quests`. Inside `QuestManager`: `HandleInventoryChanged()` → `progress.CheckInventory()`, `HandleDayOrDiveChanged()` → `progress.CheckDayOrDive()`, and the effect helpers → `QuestStepEffects.X`.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode.

## Global Constraints

- Roadmap: `docs/uml/findings.md` §4 Phase 5 (one class per PR; this is the QuestManager PR).
- No behaviour change, public API unchanged, `SaveData`/`QuestSaveData` unchanged.
- Move bodies verbatim, comments included (renames only as listed above). Comment clean-up is Phase 6, not this PR.
- Duplicate `QuestManager` (destroyed in `Awake`): it no longer subscribes to the 9 events. Today it subscribes with an empty quest list, so every handler is a no-op, so this is not a behaviour change.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`.
- Back up the save folder before Play and restore it byte-identical after.

## Risks

- **Re-entrancy:** `CompleteProgress` → `CheckInventory` → `completeProgress` recursion is the same path as today, only through a delegate.
- **Subscription timing:** `Awake` runs before `OnEnable` on the same object, so `progress` exists when `Subscribe()` runs.
- **Static event raise:** only `QuestManager` can raise `OnQuestUpdated`, so the tracker gets it as a delegate.

---

### Task 1: Tests first

**Files:** Create `Assets/Project_Ocean/Tests/EditMode/QuestSplitTests.cs`

- [ ] **Step 1:** Write tests against the new API (red until Task 2):
  - `RemoveQuestItems_CombinedTotal_TakesGreedilyInListOrder`: fake `IInventory` holding item 1 ×2 and item 2 ×5; step `Delivery`, `InventoryHeld`, `CombinedTotal`, `ConsumeOnComplete = true`, `TargetQuestItemIds = {"1","2"}`, `TargetCount = 4`. Expect removals `(1,2)` then `(2,2)`.
  - `RemoveQuestItems_ConsumeOff_RemovesNothing`.
  - `MoneyRequirementMet_NoRequiredMoney_IsTrue`.
  - `SaveMapper_RoundTrip_RestoresStatusStepAndCount`: needs a `Quest`; build it from `ScriptableObject.CreateInstance<QuestData>()` with 2 steps. If `Quest` can't be built from a bare `QuestData`, drop this test and say so in the report.
- [ ] **Step 2:** `refresh_unity`, `read_console`: expected red `CS0103`/`CS0246` for the missing types.

### Task 2: Extract

- [ ] **Step 1:** Create `QuestStepEffects.cs` and `QuestSaveMapper.cs` (bodies moved verbatim).
- [ ] **Step 2:** Create `QuestProgressTracker.cs` (bodies moved verbatim, renames as in **Wiring**).
- [ ] **Step 3:** Edit `QuestManager.cs`: delete the moved members, add the wiring, point the remaining calls at the new classes.
- [ ] **Step 4:** `refresh_unity` (force, compile), `read_console` `error CS` = 0, `run_tests` EditMode: 23 + new tests, all pass. `wc -l QuestManager.cs` ≈ 480.

### Task 3: Verify live, docs

- [ ] **Step 1:** Missing-script scan (prefabs and scenes, as in Phase 4 Task 3 Step 1). Expected 0 (no MonoBehaviour changed, so this is a sanity check).
- [ ] **Step 2:** Play smoke (save backup first): load slot 0, then check one of each path live:
  - Talk step completes on NPC close (`HandleNpcTalk`).
  - Enemy kill bounty counts (`EnemyEvents` → `HandleEnemyKilled`).
  - InventoryHeld delivery counts up when an item is added (`Inventory.OnChanged`).
  - `DevSkipCurrentStep` advances and pays the reward (`OnMoneyRewarded` → `PlayerMoney`).
  - Save → load restores the quest step and count.
  Console: no new errors. Exit Play, restore saves (RESTORED), `git status --short -- "*.unity" "*.prefab"` empty.
- [ ] **Step 3:** `python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py`, `graphify update .`. In `docs/uml/findings.md` Phase 5, mark the `QuestManager` row done with the new line count.
- [ ] **Step 4:** Hand-off. Suggested commit: "refactor(quest): split QuestManager into progress tracker, step effects and save mapper". Do not commit.
