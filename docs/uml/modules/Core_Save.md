# Core/Save

7 types · 246 LOC (non-blank) · depends on: [Core](Core.md) ×2

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class EncryptionHelper {
    <<static>>
    +Encrypt(string)$ string
    +Decrypt(string)$ string
  }
  class ISaveParticipant {
    <<interface>>
    +Collect(SaveData)* void
    +Apply(SaveData)* void
  }
  class SaveData {
    +int saveVersion
    +GameState gameState
    +string currentScene
    +float posX
    +float posY
    +float posZ
    +string saveTime
    +bool miniHubAvailable
    +float maxRopeLength
    +float reduceMaxRopeLength
    ... +17 more fields
  }
  class ItemSaveData {
    +int slotIndex
    +string itemID
    +int count
  }
  class QuestSaveData {
    +int questId
    +int currentCount
    +int status
    +int currentStepIndex
    +int dayStepBecameCurrent
  }
  class SaveSlotMeta {
    +int slot
    +string saveTime
    +int currentDay
    +int money
    +GameState gameState
  }
  class SaveSystem {
    <<static>>
    +int SAVE_VERSION$
    +Save(SaveData, int)$ void
    +Load(int)$ SaveData
    +HasSave(int)$ bool
    +Delete(int, bool)$ void
    +RestoreBackup(int)$ bool
    +GetAllSlotMeta(int)$ List~SaveSlotMeta~
  }
  SaveData --> ItemSaveData : inventoryItems
  SaveData --> QuestSaveData : quests
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| EncryptionHelper | static | `EncryptionHelper.cs:8` | 41 |
| ISaveParticipant | interface | `ISaveParticipant.cs:9` | 5 |
| ItemSaveData | class | `SaveData.cs:61` | 6 |
| QuestSaveData | class | `SaveData.cs:69` | 8 |
| SaveData | class | `SaveData.cs:7` | 28 |
| SaveSlotMeta | class | `SaveSystem.cs:14` | 8 |
| SaveSystem | static | `SaveSystem.cs:23` | 150 |
