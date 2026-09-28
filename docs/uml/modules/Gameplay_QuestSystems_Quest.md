# Gameplay/QuestSystems/Quest

33 types · 1709 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×23, [Core](Core.md) ×9, [Gameplay/Game](Gameplay_Game.md) ×4, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×3, [Gameplay/Inventory](Gameplay_Inventory.md) ×3, [Core/Save](Core_Save.md) ×3, [Gameplay/Environments](Gameplay_Environments.md) ×3, [Gameplay/Contracts](Gameplay_Contracts.md) ×2, [Core/Scenes](Core_Scenes.md) ×2, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×2, [Gameplay/Interaction](Gameplay_Interaction.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [Core/Save/DayReset](Core_Save_DayReset.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1, [Core/Datas/Items](Core_Datas_Items.md) ×1

[← index](../README.md)

## part 1

```mermaid
classDiagram
  direction LR
  class ExploreAreaTrigger {
    <<MB>>
    +FindByAreaId(string)$ Transform
  }
  class Mode {
    <<enum>>
    Interact
    TriggerZone
  }
  class DialogueLine {
    <<struct>>
    +string speakerId
    +string text
  }
  class PlayerInterior {
    <<enum>>
    None
    House
    Shop
    UpgradeShop
  }
  class PlayerInteriorState {
    <<static>>
    +PlayerInterior Current$
    +TutorialActionId CurrentExitAction$
    +EntryActionFor(TutorialActionId)$ TutorialActionId
  }
  class PlayerSelfDialogueUI {
    <<MB>>
    +bool BlocksInteraction$
  }
  class Line {
    <<struct>>
    +string speakerId
    +string text
  }
  class QuestType {
    <<enum>>
    Delivery
    ExploreArea
    Talk
    Tutorial
    Cutscene
    WaitingQuest
  }
  class DeliverySource {
    <<enum>>
    InventoryHeld
    EnemyKills
  }
  class QuestStatus {
    <<enum>>
    New
    InProgress
    Finish
    ReadyToTurnIn
    Reject
  }
  class QuestStep {
    +QuestType Type
    +string TargetEnemyId
    +List~string~ TargetEnemyIds
    +TutorialActionId TutorialAction
    +string TargetQuestItemId
    +List~string~ TargetQuestItemIds
    +List~QuestItemRequirement~ TargetQuestItemRequirements
    +DeliveryCountMode DeliveryCountMode
    +DeliverySource DeliverySource
    +bool ConsumeOnComplete
    ... +31 more fields
    +MarkNpcTalkedTo(string) bool
    +ComputeDeliveryProgress(Func~string, int~) int
    +SetDialogue(List~QuestDialogueLine~) void
    +GetLines(string, DialogueType) string
    +GetLine(string, DialogueType, int) string
    +GetCutsceneId(string, DialogueType) string
  }
  class Quest {
    +int QuestID
    +QuestCategory Category
    +QuestClassification Classification
    +QuestStatus Status
    +int CurrentStepIndex
    +int StepCount
    +QuestStep CurrentStep
    +string StepDescription
    +int TargetCount
    +int CurrentCount
    ... +12 more fields
    +Quest(QuestData)
    +AddProgress(int) bool
    +GetCompletedSteps() IEnumerable~QuestStep~
  }
  class QuestCompletionMode {
    <<enum>>
    AutoComplete
    ReadyToHandIn
  }
  class QuestCategory {
    <<enum>>
    FishS
    FishM
    FishL
    Trash
    Explore
  }
  class QuestClassification {
    <<enum>>
    MainQuest
    SubQuest
    Special
  }
  class NextQuestMode {
    <<enum>>
    None
    Unlock
    AutoAccept
  }
  class DialogueType {
    <<enum>>
    Offer
    InProgress
    Finish
    NotFinish
    Accept
    Reject
  }
  class DeliveryCountMode {
    <<enum>>
    CombinedTotal
    PerItem
  }
  class TalkCompletionMode {
    <<enum>>
    AnyOne
    All
  }
  class QuestItemRequirement {
    +string itemId
    +int count
  }
  class QuestDialogueLine {
    +string characterId
    +DialogueType type
    +string lines
    +string cutsceneId
  }
  class QuestStepData {
    +QuestType type
    +string targetEnemyId
    +List~string~ targetEnemyIds
    +TutorialActionId tutorialAction
    +string targetQuestItemId
    +DeliverySource deliverySource
    +List~string~ targetQuestItemIds
    +List~QuestItemRequirement~ targetQuestItemRequirements
    +DeliveryCountMode deliveryCountMode
    +bool consumeOnComplete
    ... +15 more fields
  }
  class QuestData {
    <<SO>>
    +QuestCategory category
    +QuestClassification classification
    +int questId
    +QuestStatus status
    +NextQuestMode nextQuestMode
    +List~int~ nextQuestIds
    +List~QuestStepData~ steps
    +string questName
    +string questGiverNpcId
  }
  class QuestManager {
    <<MB>>
    +QuestManager Instance$
    +event Action OnQuestUpdated$
    +event Action~Quest~ OnQuestCompleted$
    +event Action~int~ OnMoneyRewarded$
    +ResetToDefault() void
    +CompleteAutoDialogue(Quest) void
    +IsUnlocked(Quest) bool
    +GetAllQuests() List~Quest~
    +GetQuestsByStatus(QuestStatus) List~Quest~
    +GetQuestsByCategory(QuestCategory) List~Quest~
    +GetActiveQuests() List~Quest~
    +GetQuestById(int) Quest
    +GetMainQuestTalkQuest(string) Quest
    +GetSaveState() List~QuestSaveData~
    +GetCompletedQuests() List~Quest~
    +LoadSaveState(List~QuestSaveData~) void
    ... +11 more methods()
  }
  class QuestNavigation {
    <<static>>
    +float CloseRangeMeters$
    +InteractPosition(Transform)$ Vector3
    +ResolveQuestTarget()$ Transform
    +ResolveCompassTarget()$ Transform
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  ExploreAreaTrigger --> DialogueLine : dialogueLines
  ExploreAreaTrigger --> Mode : mode
  ISaveParticipant <|.. QuestManager
  PlayerInteriorState --> PlayerInterior : Current
  PlayerSelfDialogueUI --> Line : lines
  PlayerSelfDialogueUI --> Quest : activeQuest
  Quest --> NextQuestMode : NextQuestMode
  Quest --> QuestCategory : category
  Quest --> QuestClassification : classification
  Quest --> QuestItemRequirement : TargetQuestItemRequirements
  Quest --> QuestStatus : status
  Quest --> QuestStep : steps
  QuestData --> NextQuestMode : nextQuestMode
  QuestData --> QuestCategory : category
  QuestData --> QuestClassification : classification
  QuestData --> QuestStatus : status
  QuestData --> QuestStepData : steps
  QuestDialogueLine --> DialogueType : type
  QuestManager --> Quest : activeQuestList
  QuestManager --> QuestData : allQuestTemplates
  QuestStep --> DeliveryCountMode : DeliveryCountMode
  QuestStep --> DeliverySource : DeliverySource
  QuestStep --> DialogueType : dialogue
  QuestStep --> QuestCompletionMode : CompletionMode
  QuestStep --> QuestDialogueLine : orderedDialogue
  QuestStep --> QuestItemRequirement : TargetQuestItemRequirements
  QuestStep --> QuestType : Type
  QuestStep --> TalkCompletionMode : TalkCompletionMode
  QuestStepData --> DeliveryCountMode : deliveryCountMode
  QuestStepData --> DeliverySource : deliverySource
  QuestStepData --> QuestCompletionMode : completionMode
  QuestStepData --> QuestDialogueLine : dialogue
  QuestStepData --> QuestItemRequirement : targetQuestItemRequirements
  QuestStepData --> QuestType : type
  QuestStepData --> TalkCompletionMode : talkCompletionMode
```

## part 2

```mermaid
classDiagram
  direction LR
  class QuestVisuals {
    <<static>>
    +GetItemSprite(string)$ Sprite
    +Apply(Image, Sprite)$ void
    +GetDeliveryStatusSuffix(Quest)$ string
  }
  class ReviceQuestListUI {
    <<MB>>
  }
  class ReviceQuestSlot {
    <<MB>>
    +Setup(Quest) void
  }
  class TutorialActionMarker {
    <<MB>>
    +FindByTutorialAction(TutorialActionId)$ Transform
  }
  class TutorialGate {
    <<static>>
    +TutorialActionId RequiredAction$
    +bool IsRestricting$
    +Blocks(TutorialActionId)$ bool
    +NotifyBlocked()$ void
  }
  class TutorialStep {
    <<static>>
    +IsActive(TutorialActionId)$ bool
    +BlocksShop(TutorialActionId)$ bool
  }
  class TutorialPromptUI {
    <<MB>>
    +string CurrentPrompt$
  }
  class TutorialUIMarker {
    <<MB>>
  }
  ReviceQuestListUI --> ReviceQuestSlot : slotPrefab
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| ExploreAreaTrigger | QuestManager | OnQuestUpdated |
| PlayerInteriorState | TutorialEvents | OnActionPerformed |
| PlayerSelfDialogueUI | InputReader | OnInteracted |
| PlayerSelfDialogueUI | LoadingManager | OnLoadingFinished |
| PlayerSelfDialogueUI | QuestManager | OnQuestUpdated |
| QuestManager | CutsceneEvents | OnCutsceneFinished |
| QuestManager | DayManager | OnDayAdvanced |
| QuestManager | DayManager | OnDivedToday |
| QuestManager | DialogueState | OnChanged |
| QuestManager | EnemyEvents | OnEnemyKilled |
| QuestManager | ExploreEvents | OnAreaReached |
| QuestManager | NpcEvents | OnNpcTalkedTo |
| QuestManager | PlayerMoney | OnMoneyChanged |
| QuestManager | TutorialEvents | OnActionPerformed |
| ReviceQuestListUI | QuestManager | OnQuestUpdated |
| TutorialActionMarker | QuestManager | OnQuestUpdated |
| TutorialPromptUI | LoadingManager | OnLoadingFinished |
| TutorialPromptUI | PlayerUIState | OnLockChanged |
| TutorialPromptUI | QuestManager | OnQuestUpdated |
| TutorialUIMarker | QuestManager | OnQuestUpdated |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DeliveryCountMode | enum | `QuestData.cs:54` | 5 |
| DeliverySource | enum | `Quest.cs:28` | 5 |
| DialogueLine | struct | `ExploreAreaTrigger.cs:50` | 1 |
| DialogueType | enum | `QuestData.cs:40` | 9 |
| ExploreAreaTrigger | MB | `ExploreAreaTrigger.cs:23` | 171 |
| Line | struct | `PlayerSelfDialogueUI.cs:40` | 1 |
| Mode | enum | `ExploreAreaTrigger.cs:24` | 5 |
| NextQuestMode | enum | `QuestData.cs:32` | 6 |
| PlayerInterior | enum | `PlayerInteriorState.cs:5` | 1 |
| PlayerInteriorState | static | `PlayerInteriorState.cs:13` | 36 |
| PlayerSelfDialogueUI | MB | `PlayerSelfDialogueUI.cs:21` | 128 |
| Quest | class | `Quest.cs:292` | 163 |
| QuestCategory | enum | `QuestData.cs:14` | 8 |
| QuestClassification | enum | `QuestData.cs:24` | 6 |
| QuestCompletionMode | enum | `QuestData.cs:7` | 5 |
| QuestData | SO | `QuestData.cs:178` | 19 |
| QuestDialogueLine | class | `QuestData.cs:89` | 10 |
| QuestItemRequirement | class | `QuestData.cs:75` | 7 |
| QuestManager | MB | `QuestManager.cs:8` | 585 |
| QuestNavigation | static | `QuestNavigation.cs:9` | 39 |
| QuestStatus | enum | `Quest.cs:34` | 8 |
| QuestStep | class | `Quest.cs:48` | 117 |
| QuestStepData | class | `QuestData.cs:108` | 47 |
| QuestType | enum | `Quest.cs:8` | 9 |
| QuestVisuals | static | `QuestVisuals.cs:11` | 53 |
| ReviceQuestListUI | MB | `ReviceQuestListUI.cs:14` | 52 |
| ReviceQuestSlot | MB | `ReviceQuestSlot.cs:8` | 28 |
| TalkCompletionMode | enum | `QuestData.cs:65` | 5 |
| TutorialActionMarker | MB | `TutorialActionMarker.cs:14` | 42 |
| TutorialGate | static | `TutorialGate.cs:11` | 21 |
| TutorialPromptUI | MB | `TutorialPromptUI.cs:23` | 78 |
| TutorialStep | static | `TutorialGate.cs:58` | 24 |
| TutorialUIMarker | MB | `TutorialUIMarker.cs:15` | 22 |
