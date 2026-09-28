# Cleanup Phase 6e: Items Feature Folder, Checkpoint to Deprecated — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal (user, 2026-09-29):** live folders hold only code the game uses, and all item code sits together, so a future item "like the MiniHub" is found in one place. The unused old `MiniHub.cs` goes away so it can't be confused with the live `MiniHubItem`.

**Audit:** all 221 scripts outside `Deprecated/` and `Editor/` are referenced (by an asset or by code). The only unused live code is the checkpoint item (disabled behind `#if ENABLE_CHECKPOINT_ITEM` in Phase 5c).

## Moves (Unity `AssetDatabase.MoveAsset`, so GUIDs and prefab/scene links are kept)

Namespaces follow the layer (Phase 4), not the subfolder, so moving inside `Gameplay/` changes no code.

| From | To |
|---|---|
| `Scripts/Gameplay/InteractObjects/IItemEffect.cs` | `Scripts/Gameplay/Items/IItemEffect.cs` |
| `Scripts/Gameplay/Entities/Players/PlayerItemUse.cs` | `Scripts/Gameplay/Items/PlayerItemUse.cs` |
| `Scripts/Gameplay/InteractObjects/MiniHubItem.cs` | `Scripts/Gameplay/Items/Placeables/MiniHubItem.cs` |
| `Scripts/Gameplay/InteractObjects/CollectableItemBase.cs` | `Scripts/Gameplay/Items/Pickups/CollectableItemBase.cs` |
| `Scripts/Gameplay/InteractObjects/ItemDrop.cs` | `Scripts/Gameplay/Items/Pickups/ItemDrop.cs` |
| `Scripts/Gameplay/InteractObjects/ShelleController.cs` | `Scripts/Gameplay/Items/Pickups/ShelleController.cs` |
| `Scripts/Gameplay/InteractObjects/ObjectWrapPoint.cs` | `Scripts/Gameplay/Entities/Players/OxygenTube/ObjectWrapPoint.cs` (rope geometry; its only user is `RopeCornerFinder` there) |
| `Scripts/Gameplay/InteractObjects/CheckPointItem.cs` | `Scripts/Deprecated/CheckpointItem/CheckPointItem.cs` |
| `Prefabs/Interactions/Items/CheckPoint.prefab` | `Scripts/Deprecated/CheckpointItem/CheckPoint.prefab` |
| `Scripts/Core/Datas/ScriptableDatas/Items/Buildings/ItemCheckpoint.asset` | `Scripts/Deprecated/CheckpointItem/ItemCheckpoint.asset` |

`Gameplay/InteractObjects/` is then empty and gets deleted. `ItemSpawner.cs` stays in `Spawning/` (it's part of the spawning system), and `Inventory.cs`/`IInventory.cs` stay in `Gameplay/Inventory/`.

## Delete
- `Scripts/Deprecated/MiniHub.cs` (+ `.meta`): old design, no references. Git keeps the history.

## Comment updates (text only)
- The `#if ENABLE_CHECKPOINT_ITEM` blocks in `Player.cs`, `OxygenRope.cs` and `InventoryManager.cs` stay (the user asked to keep the checkpoint code). Their comment changes to: re-enabling means moving `Deprecated/CheckpointItem/CheckPointItem.cs` back into `Gameplay/Items/` **and** adding the define, because `Game.Deprecated` is editor-only and `Player` can't see it.
- `CheckPointItem.cs` header: the same note.

## Checks
- GUID snapshot before/after: 10 moved, 1 deleted (`MiniHub.cs`), 0 missing otherwise.
- Compile 0 errors, tests 36/36, missing-script scan 0 (the `MiniHubPoint`, `ItemDrop` and shell prefabs stay linked). `CheckPoint.prefab` keeps its "class not compiled" state, as before.
- Play: in the ocean, the MiniHub spawns and ties the rope; an item pickup still collects. Restore saves.
- `findings.md` note, UML regen + tests, `graphify update .`. Suggested commit: "chore: Items feature folder; checkpoint item to Deprecated; drop old MiniHub.cs". Do not commit.
