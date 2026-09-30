# Gameplay/QuestSystems/Quest

36 types · 1717 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×22, [Core](Core.md) ×13, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×5, [Core/Save](Core_Save.md) ×4, [Gameplay/Inventory](Gameplay_Inventory.md) ×4, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×3, [Gameplay/Environments](Gameplay_Environments.md) ×3, [Core/Save/DayReset](Core_Save_DayReset.md) ×2, [Core/Scenes](Core_Scenes.md) ×2, [Gameplay/Contracts](Gameplay_Contracts.md) ×2, [Core/Datas/Items](Core_Datas_Items.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [Gameplay/Interaction](Gameplay_Interaction.md) ×1, [Gameplay/Services](Gameplay_Services.md) ×1

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
    +event Action OnQuestUpdated$
    +event Action~Quest~ OnQuestCompleted$
    #bool Persistent
    #Awake() void
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
    ... +12 more methods()
  }
  class QuestMarkerBase {
    <<MB>>
    <<abstract>>
    #GameObject markerUI
    #bool PollEveryFrame
    #OnEnable() void
    #OnDisable() void
    #Refresh() void
    #ShouldShow()* bool
    #SetVisible(bool) void
  }
  class QuestNavigation {
    <<static>>
    +float CloseRangeMeters$
    +InteractPosition(Transform)$ Vector3
    +CurrentTutorialAction()$ TutorialActionId
    +IsPlayerNear(Transform)$ bool
    +ResolveQuestTarget()$ Transform
    +ResolveCompassTarget()$ Transform
  }
  class ISaveParticipant {
    <<Core/Save>>
  }
  class Singleton {
    <<Core>>
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
  QuestData --> NextQuestMode : nextQuestMode
  QuestData --> QuestCategory : category
  QuestData --> QuestClassification : classification
  QuestData --> QuestStatus : status
  QuestData --> QuestStepData : steps
  QuestDialogueLine --> DialogueType : type
  QuestManager --> Quest : activeQuestList
  QuestManager --> QuestData : allQuestTemplates
  QuestStepData --> DeliveryCountMode : deliveryCountMode
  QuestStepData --> DeliverySource : deliverySource
  QuestStepData --> QuestCompletionMode : completionMode
  QuestStepData --> QuestDialogueLine : dialogue
  QuestStepData --> QuestItemRequirement : targetQuestItemRequirements
  QuestStepData --> QuestType : type
  QuestStepData --> TalkCompletionMode : talkCompletionMode
  Singleton <|-- QuestManager
```

## part 2

```mermaid
classDiagram
  direction LR
  class QuestProgressTracker {
    +QuestProgressTracker(List~Quest~, Action~Quest, int~, Action)
    +Subscribe() void
    +Unsubscribe() void
    +CheckDayOrDive() void
    +CheckInventory() void
  }
  class QuestSaveMapper {
    <<static>>
    +ToSaveData(IEnumerable~Quest~)$ List~QuestSaveData~
    +Apply(List~QuestSaveData~, Func~int, Quest~)$ void
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
  class QuestStepEffects {
    <<static>>
    +MoneyRequirementMet(QuestStep)$ bool
    +StampWaitingStart(QuestStep)$ void
    +ApplyFeatureChanges(QuestStep)$ void
    +RemoveQuestItems(QuestStep)$ void
  }
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
    #OnEnable() void
    #OnDisable() void
    #ShouldShow() bool
  }
  class TutorialPromptUI {
    <<MB>>
  }
  class TutorialStep {
    <<static>>
    +IsActive(TutorialActionId)$ bool
    +BlocksShop(TutorialActionId)$ bool
  }
  class TutorialUIMarker {
    <<MB>>
    #bool PollEveryFrame
    #ShouldShow() bool
    #SetVisible(bool) void
  }
  class QuestMarkerBase {
    <<Gameplay/QuestSystems/Quest>>
  }
  QuestMarkerBase <|-- TutorialActionMarker
  QuestMarkerBase <|-- TutorialUIMarker
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
| QuestMarkerBase | QuestManager | OnQuestUpdated |
| QuestProgressTracker | CutsceneEvents | OnCutsceneFinished |
| QuestProgressTracker | DayManager | OnDayAdvanced |
| QuestProgressTracker | DayManager | OnDivedToday |
| QuestProgressTracker | DialogueState | OnChanged |
| QuestProgressTracker | EnemyEvents | OnEnemyKilled |
| QuestProgressTracker | ExploreEvents | OnAreaReached |
| QuestProgressTracker | NpcEvents | OnNpcTalkedTo |
| QuestProgressTracker | PlayerMoney | OnMoneyChanged |
| QuestProgressTracker | TutorialEvents | OnActionPerformed |
| ReviceQuestListUI | QuestManager | OnQuestUpdated |
| TutorialPromptUI | LoadingManager | OnLoadingFinished |
| TutorialPromptUI | PlayerUIState | OnLockChanged |
| TutorialPromptUI | QuestManager | OnQuestUpdated |

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
| PlayerSelfDialogueUI | MB | `PlayerSelfDialogueUI.cs:21` | 124 |
| Quest | class | `Quest.cs:44` | 163 |
| QuestCategory | enum | `QuestData.cs:14` | 8 |
| QuestClassification | enum | `QuestData.cs:24` | 6 |
| QuestCompletionMode | enum | `QuestData.cs:7` | 5 |
| QuestData | SO | `QuestData.cs:178` | 19 |
| QuestDialogueLine | class | `QuestData.cs:89` | 10 |
| QuestItemRequirement | class | `QuestData.cs:75` | 7 |
| QuestManager | MB | `QuestManager.cs:8` | 328 |
| QuestMarkerBase | MB, abstract | `QuestMarkerBase.cs:6` | 23 |
| QuestNavigation | static | `QuestNavigation.cs:9` | 49 |
| QuestProgressTracker | class | `QuestProgressTracker.cs:8` | 179 |
| QuestSaveMapper | static | `QuestSaveMapper.cs:8` | 32 |
| QuestStatus | enum | `Quest.cs:34` | 8 |
| QuestStep | class | `QuestStep.cs:13` | 117 |
| QuestStepData | class | `QuestData.cs:108` | 47 |
| QuestStepEffects | static | `QuestStepEffects.cs:7` | 68 |
| QuestType | enum | `Quest.cs:8` | 9 |
| QuestVisuals | static | `QuestVisuals.cs:11` | 53 |
| ReviceQuestListUI | MB | `ReviceQuestListUI.cs:14` | 52 |
| ReviceQuestSlot | MB | `ReviceQuestSlot.cs:8` | 28 |
| TalkCompletionMode | enum | `QuestData.cs:65` | 5 |
| TutorialActionMarker | MB | `TutorialActionMarker.cs:8` | 29 |
| TutorialPromptUI | MB | `TutorialPromptUI.cs:23` | 77 |
| TutorialStep | static | `TutorialStep.cs:10` | 24 |
| TutorialUIMarker | MB | `TutorialUIMarker.cs:9` | 14 |
