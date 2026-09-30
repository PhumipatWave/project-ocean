# Gameplay/Save

1 types · 183 LOC (non-blank) · depends on: [Core/Save](Core_Save.md) ×5, [Core](Core.md) ×3, [Core/Save/DayReset](Core_Save_DayReset.md) ×3, [Core/Scenes](Core_Scenes.md) ×3, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×3, [Gameplay/Services](Gameplay_Services.md) ×2, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×1, [Gameplay/Game](Gameplay_Game.md) ×1, [Gameplay/Inventory](Gameplay_Inventory.md) ×1, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class SaveBridge {
    <<MB>>
    +event System.Action OnSaved$
    +int CurrentSlot
    #bool DestroyGameObjectOnDuplicate
    +ResetForNewGame() void
    +BeginNewGameSession(int) void
    +SaveGame() void
    +SaveGame(int) void
    +LoadGame() void
    +LoadGame(int) void
    +GetAllSlotMeta(int) List~SaveSlotMeta~
  }
  class Singleton {
    <<Core>>
  }
  Singleton <|-- SaveBridge
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| SaveBridge | DayManager | OnDayAdvanced |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| SaveBridge | MB | `SaveBridge.cs:9` | 183 |
