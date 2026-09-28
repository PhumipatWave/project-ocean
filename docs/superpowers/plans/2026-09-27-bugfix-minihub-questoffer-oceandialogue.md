# Bug Fixes: Mini Hub Slot Save, NPC Quest-Offer Panel, Ocean Explore Dialogue — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix three existing bugs found during the Phase 3 live checks.

**Root causes (verified 2026-09-27):**
1. **Mini Hub lost on load.** `InventoryManager` has 14 `inventorySlots` and 1 separate `buildingSlots` entry (no overlap, checked in `AS_PlayerHUD.unity`). `InventoryManager.Collect/Apply` only walk `inventorySlots`, so an item in the building slot is never saved.
2. **NPC quest offer never visible.** NPCs ShopF, Hunter and Femboy reference `LandUI/QuestBoardPanel/QuestDetailBoard`. `QuestBoardPanel` is inactive whenever the board is closed, so `QuestDetailPanel.ShowPanel` activates a child of an inactive parent. It has been under the board panel since at least June (`e14967b`).
3. **Ocean explore dialogue skipped.** `MQ_1002` step 13 (ExploreArea `AR001`) authors 1 dialogue line. Ocean_Map's `ExploreAreaTrigger` "Area" (`AR001`, Interact mode) has no `dialogueView`, because Ocean_Map has no dialogue panel, so the documented "no panel → reached immediately" fallback swallows the line.

**Architecture:**
1. Additive save field `buildingItems` (JsonUtility keeps the initializer for old files, so old saves load with an empty list; no version bump). `InventoryManager` collects and applies it next to `inventoryItems`.
2. Give NPCs their own offer panel: an editor duplicate of `QuestDetailBoard` placed under `LandUI` (outside `QuestBoardPanel`), inactive by default, and wire the three NPCs to it. The board keeps its own.
3. Add an `InteractDialoguePanel.prefab` instance to Ocean_Map's HUD canvas with a `DialoguePanelView`, wired to the trigger "Area".

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP, NUnit EditMode.

## Global Constraints

- The user asked for these three fixes on 2026-09-27, before Phase 4.
- Scene edits only through `execute_code` using `UnityEditor` APIs. Save scenes with `EditorSceneManager.SaveScene(scene)` (never `manage_scene save`, memory `feedback_unity_mcp_scene_save_bug.md`). Check `git status --short -- "*.unity" "*.prefab"` after each save: only the intended scene paths.
- Old saves must still load. No existing `SaveData` field changes.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before Play and restore it byte-identical after.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout.
- Comment rules (CLAUDE.md).

---

### Task 1: Save the building (Mini Hub) slot

**Files:**
- Modify: `Assets/Project_Ocean/Scripts/Core/Save/SaveData.cs`, `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs`
- Test: `Assets/Project_Ocean/Tests/EditMode/SaveDataCompatTests.cs`

- [ ] **Step 1: Failing test (old save JSON loads with an empty building list)**

```csharp
using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SaveDataCompatTests
    {
        [Test]
        public void OldSaveWithoutBuildingItems_LoadsWithEmptyList()
        {
            var data = JsonUtility.FromJson<SaveData>("{\"money\":5}");
            Assert.IsNotNull(data.buildingItems);
            Assert.AreEqual(0, data.buildingItems.Count);
            Assert.AreEqual(5, data.money);
        }

        [Test]
        public void BuildingItems_RoundTripThroughJson()
        {
            var data = new SaveData();
            data.buildingItems.Add(new ItemSaveData { slotIndex = 0, itemID = "42", count = 1 });
            var back = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(data));
            Assert.AreEqual(1, back.buildingItems.Count);
            Assert.AreEqual("42", back.buildingItems[0].itemID);
        }
    }
}
```
`refresh_unity` + `read_console` `error CS`. Expected red: `CS1061` ("'SaveData' does not contain a definition for 'buildingItems'").

- [ ] **Step 2: `SaveData.buildingItems`**

In `SaveData.cs`, directly below `public List<ItemSaveData> inventoryItems = new();` add:
```csharp
        // Mini Hub (building) slot items; slotIndex = index into InventoryManager's building slots.
        public List<ItemSaveData> buildingItems = new();
```

- [ ] **Step 3: `InventoryManager` collects and applies the building slots**

In `InventoryManager.Collect(SaveData data)`, at the end of the method, add:
```csharp
            data.buildingItems = new List<ItemSaveData>();
            if (buildingSlots == null) return;
            for (int i = 0; i < buildingSlots.Count; i++)
            {
                var item = buildingSlots[i] != null ? buildingSlots[i].GetComponentInChildren<DraggableItem>() : null;
                if (item?.GetItem() == null) continue;
                data.buildingItems.Add(new ItemSaveData
                {
                    slotIndex = i,
                    itemID    = item.GetItem().ItemID.ToString(),
                    count     = item.count,
                });
            }
```
In `InventoryManager.Apply(SaveData data)`, at the end of the method, add:
```csharp
            if (buildingSlots == null) return;
            foreach (var slot in buildingSlots)
            {
                var existing = slot != null ? slot.GetComponentInChildren<DraggableItem>() : null;
                if (existing != null) Destroy(existing.gameObject);
            }

            // Null on saves written before building slots were saved (JsonUtility keeps the initializer, but guard anyway).
            if (data.buildingItems == null) return;
            foreach (var saved in data.buildingItems)
            {
                if (!int.TryParse(saved.itemID, out int id)) continue;
                var item = FindItemInDatabase(id);
                if (item == null)
                {
                    Debug.LogError($"[InventoryManager] Building item ID {id} not found in allItems database — dropped on load.");
                    continue;
                }
                if (saved.slotIndex < 0 || saved.slotIndex >= buildingSlots.Count || buildingSlots[saved.slotIndex] == null) continue;
                SpawnNewItem(item, buildingSlots[saved.slotIndex], Mathf.Max(1, saved.count));
            }
            RefreshBuildingHints();
```
`SaveBridge.SaveGame` needs no change: the `SaveData` initializer covers the "no InventoryManager" case.

- [ ] **Step 4: Green**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: 23/23 (21 + 2).

---

### Task 2: Scene fixes (NPC offer panel, Ocean dialogue panel)

**Files:** `Assets/Project_Ocean/Levels/Scenes/Lands/Land_Map.unity`, `Assets/Project_Ocean/Levels/Scenes/Oceans/Ocean_Map.unity` (editor edits only; no code).

- [ ] **Step 1: Snapshot**

Run `git status --short -- "*.unity" "*.prefab"` and record it. Expected: `Land_Map.unity` modified (the Phase 3f-2 migration, uncommitted).

- [ ] **Step 2: Land_Map — dedicated NPC quest-offer panel**

`execute_code` (not in Play). Open Land_Map (additively if needed), then:
1. Find `LandUI/QuestBoardPanel/QuestDetailBoard` (a `QuestDetailPanel`). Duplicate it with the editor's own duplicate so any prefab connection and overrides survive: `Selection.activeGameObject = go; Unsupported.DuplicateGameObjectsUsingPasteboard();`, then `var copy = Selection.activeGameObject;`. If that API is unavailable, use `Object.Instantiate(go)` and note that the copy is unpacked.
2. Rename the copy to `NpcQuestOfferPanel`. Reparent it with `copy.transform.SetParent(<LandUI transform>, worldPositionStays: true)` so it keeps its on-screen spot, and put it as the last sibling so it renders on top. Leave it inactive (`SetActive(false)`), same as the original.
3. For every `NPC` in the scene whose `questDetailPanel` currently references the original `QuestDetailPanel`, set `questDetailPanel` to the copy's `QuestDetailPanel` via `SerializedObject`. For prefab instances, call `PrefabUtility.RecordPrefabInstancePropertyModifications`. Expect exactly 3 (ShopF, Hunter, Femboy).
4. `EditorSceneManager.MarkSceneDirty(scene)` and `EditorSceneManager.SaveScene(scene)`.
5. Read back: print each NPC → `questDetailPanel` path, and the copy's ancestor chain with activeSelf. Expected: `LandUI/NpcQuestOfferPanel[OFF]`, with LandUI active.

- [ ] **Step 3: Ocean_Map — dialogue panel for ExploreAreaTrigger "Area"**

`execute_code`. Open Ocean_Map (additively), then:
1. Find the HUD canvas that ocean UI lives on: the root `Canvas` (not world-space) with the most UI children, or one named like `OceanUI`/`OceanHUD`. Print all root canvases with their names and render modes, and pick the screen-space one used by other ocean UI. If there is none, or it's ambiguous, STOP and report NEEDS_CONTEXT with the list.
2. `PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project_Ocean/Prefabs/UI/Land_UI_Panel/InteractDialoguePanel/InteractDialoguePanel.prefab"), <canvas transform>)`. Name it `InteractDialoguePanel`, set it inactive, last sibling.
3. Add a `DialoguePanelView` on the instance (an added-component override). Set `panel` to the instance GameObject. Set `nameText`, `bodyText` and `avatar` by copying the same child paths the Land_Map `DialoguePanelView` (on `LandUI/InteractDialoguePanel`) uses. Read that view's fields in Land_Map, compute each widget's path relative to its panel root, and resolve the same relative path under the new instance. Print all four resolved paths. Any unresolved path means STOP and NEEDS_CONTEXT.
4. Set the trigger "Area" (`ExploreAreaTrigger`, areaId `AR001`) `dialogueView` to the new view (`SerializedObject`; `RecordPrefabInstancePropertyModifications` if it's a prefab instance).
5. Mark dirty, `EditorSceneManager.SaveScene(scene)`, and close any scenes this step opened.

- [ ] **Step 4: Dirty check**

Run `git status --short -- "*.unity" "*.prefab"`. Expected: `Land_Map.unity` and `Ocean_Map.unity` only, both at their real paths.

---

### Task 3: Verify live, docs

- [ ] **Step 1: Back up saves** (to `$TEMP/bugfix_savebackup`).

- [ ] **Step 2: Play checks** (memories `feedback_unity_play_mode_unfocused.md`, `reference_live_enemy_test_route.md`, `project_quest_dialogue_architecture.md`)

1. **Mini Hub save:** new game (slot 9) → Land_Map. Put a Building item into the building slot (the Mini Hub item from `allItems` with `ItemType.Building`, via the same path the shop or `AddItem` uses). `SaveGame(9)`, remove the item (`ClearBuildingSlot(0)`), `LoadGame(9)`. Expected: the building slot holds the item again, with the same ID and count, and `PeekItem(ItemType.Building)` is not null. Also load an older slot from the backup (any `gamesave_slot*.json`) and check it loads without errors: the building slot is empty and the inventory is unchanged.
2. **NPC offer panel:** talk to an NPC with an unaccepted side quest whose offer ends in the panel (or force: set up an NPC with `targetQuest` in the New state; use ShopF, Hunter or Femboy with their quest state). After the last offer line, `NpcQuestOfferPanel` is `activeInHierarchy == true`. Take a screenshot to `$TEMP` and look at it: the panel is visible on screen with the quest title. Press Accept, which is `onClick.Invoke()` on the accept button. Expected: the quest is accepted and the panel hides. Walking away hides it too. The quest board still opens its own detail panel normally.
3. **Ocean explore dialogue:** get `MQ_1002` to step 13 (QuestManager dev tools such as `DevSkipCurrentStep`, per earlier runs), enter Ocean_Map, and interact with trigger "Area". Expected: the panel opens with the authored line and speaker, and the next press closes it and raises `AR001` (the step completes). Screenshot it. Stay under 3 min in Ocean.
Exit Play. Take all screenshots outside the repo.

- [ ] **Step 3: Restore, dirty check, docs**

Restore the save folder byte-identical (print RESTORED). `git status --short -- "*.unity" "*.prefab"` must list only Land_Map and Ocean_Map. Then run `python tools/uml/gen_uml.py`, `python tools/uml/test_gen_uml.py` and `graphify update .`.
In `docs/uml/findings.md` §2, add a line: `**Bugs fixed 2026-09-27:** Mini Hub building slot now saved (SaveData.buildingItems); NPC quest offer uses its own LandUI/NpcQuestOfferPanel (was hidden inside the closed QuestBoardPanel); Ocean_Map explore trigger "Area" now has a dialogue panel (MQ_1002 step 13 line was skipped).`

- [ ] **Step 4: Hand-off**

List changed files. Suggested commits: "fix(save): persist Mini Hub building slot" and "fix(scenes): visible NPC quest offer panel and ocean explore dialogue". Do not commit.
