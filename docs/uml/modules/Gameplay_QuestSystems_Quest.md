# Gameplay/QuestSystems/Quest

37 types · 1829 LOC (non-blank) · depends on: [Core](Core.md) ×13, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×9, [UI](UI.md) ×7, [Gameplay/Environments](Gameplay_Environments.md) ×4, [UI/LoadingSecne](UI_LoadingSecne.md) ×3, [Core/Save Game](Core_Save_Game.md) ×3, [UI/Notifications](UI_Notifications.md) ×3, [UI/Inventory](UI_Inventory.md) ×3, [Gameplay/Entitys/Players](Gameplay_Entitys_Players.md) ×2, [UI/Interactions](UI_Interactions.md) ×1, [Core/Inputs](Core_Inputs.md) ×1, [Core/Services](Core_Services.md) ×1, [UI/QuestCompleteUI](UI_QuestCompleteUI.md) ×1, [Core/Save Game/DayAndResetEnemys](Core_Save_Game_DayAndResetEnemys.md) ×1, [Gameplay/Entitys/Enemys](Gameplay_Entitys_Enemys.md) ×1, [Gameplay/Entitys/NPCs](Gameplay_Entitys_NPCs.md) ×1, [UI/Items](UI_Items.md) ×1

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
  class ExploreEvents {
    <<static>>
    +event Action~string~ OnAreaReached$
    +RaiseAreaReached(string)$ void
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
  class ISaveParticipant {
    <<Core/Save Game>>
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
  class QuestNavigation {
    <<static>>
    +float CloseRangeMeters$
    +InteractPosition(Transform)$ Vector3
    +ResolveQuestTarget()$ Transform
    +ResolveCompassTarget()$ Transform
  }
  class QuestTrackerUI {
    <<MB>>
    +OnClickCancelQuest() void
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
  }
  class TutorialActionId {
    <<enum>>
    Move
    Interact
    BuyItem
    SellItem
    Dive
    OpenQuestBoard
    Sleep
    MoveInHouse
    MoveInShop
    MoveInUpgradeShop
    ... +10 more
  }
  class TutorialEvents {
    <<static>>
    +event Action~TutorialActionId~ OnActionPerformed$
    +RaiseAction(TutorialActionId)$ void
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
  TutorialActionMarker --> TutorialActionId : action
  TutorialGate --> TutorialActionId : RequiredAction
  TutorialUIMarker --> TutorialActionId : action
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
| QuestManager | EnemyEvents | OnEnemyKilled |
| QuestManager | ExploreEvents | OnAreaReached |
| QuestManager | NpcEvents | OnNpcTalkedTo |
| QuestManager | PlayerMoney | OnMoneyChanged |
| QuestManager | TutorialEvents | OnActionPerformed |
| QuestTrackerUI | QuestManager | OnQuestUpdated |
| ReviceQuestListUI | QuestManager | OnQuestUpdated |
| TutorialActionMarker | QuestManager | OnQuestUpdated |
| TutorialPromptUI | LoadingManager | OnLoadingFinished |
| TutorialPromptUI | PlayerUIState | OnLockChanged |
| TutorialPromptUI | QuestManager | OnQuestUpdated |
| TutorialUIMarker | QuestManager | OnQuestUpdated |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DeliveryCountMode | enum | `QuestData.cs:53` | 5 |
| DeliverySource | enum | `Quest.cs:27` | 5 |
| DialogueLine | struct | `ExploreAreaTrigger.cs:44` | 1 |
| DialogueType | enum | `QuestData.cs:39` | 9 |
| ExploreAreaTrigger | MB | `ExploreAreaTrigger.cs:24` | 167 |
| ExploreEvents | static | `ExploreEvents.cs:5` | 8 |
| Line | struct | `PlayerSelfDialogueUI.cs:34` | 1 |
| Mode | enum | `ExploreAreaTrigger.cs:25` | 5 |
| NextQuestMode | enum | `QuestData.cs:31` | 6 |
| PlayerInterior | enum | `PlayerInteriorState.cs:3` | 1 |
| PlayerInteriorState | static | `PlayerInteriorState.cs:11` | 36 |
| PlayerSelfDialogueUI | MB | `PlayerSelfDialogueUI.cs:23` | 125 |
| Quest | class | `Quest.cs:291` | 163 |
| QuestCategory | enum | `QuestData.cs:13` | 8 |
| QuestClassification | enum | `QuestData.cs:23` | 6 |
| QuestCompletionMode | enum | `QuestData.cs:6` | 5 |
| QuestData | SO | `QuestData.cs:177` | 19 |
| QuestDialogueLine | class | `QuestData.cs:88` | 10 |
| QuestItemRequirement | class | `QuestData.cs:74` | 7 |
| QuestManager | MB | `QuestManager.cs:7` | 588 |
| QuestNavigation | static | `QuestNavigation.cs:8` | 39 |
| QuestStatus | enum | `Quest.cs:33` | 8 |
| QuestStep | class | `Quest.cs:47` | 117 |
| QuestStepData | class | `QuestData.cs:107` | 47 |
| QuestTrackerUI | MB | `QuestTrackerUI.cs:7` | 85 |
| QuestType | enum | `Quest.cs:7` | 9 |
| QuestVisuals | static | `QuestVisuals.cs:10` | 53 |
| ReviceQuestListUI | MB | `ReviceQuestListUI.cs:13` | 52 |
| ReviceQuestSlot | MB | `ReviceQuestSlot.cs:7` | 28 |
| TalkCompletionMode | enum | `QuestData.cs:64` | 5 |
| TutorialActionId | enum | `TutorialEvents.cs:6` | 23 |
| TutorialActionMarker | MB | `TutorialActionMarker.cs:13` | 42 |
| TutorialEvents | static | `TutorialEvents.cs:36` | 8 |
| TutorialGate | static | `TutorialEvents.cs:56` | 21 |
| TutorialPromptUI | MB | `TutorialPromptUI.cs:22` | 78 |
| TutorialStep | static | `TutorialEvents.cs:103` | 24 |
| TutorialUIMarker | MB | `TutorialUIMarker.cs:14` | 22 |
