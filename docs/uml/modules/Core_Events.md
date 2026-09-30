# Core/Events

12 types · 138 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class CutsceneEvents {
    <<static>>
    +event Action~string~ OnCutsceneFinished$
    +RaiseCutsceneFinished(string)$ void
  }
  class EnemyEvents {
    <<static>>
    +event System.Action~string~ OnEnemyKilled$
    +RaiseEnemyKilled(string)$ void
  }
  class ExploreEvents {
    <<static>>
    +event Action~string~ OnAreaReached$
    +RaiseAreaReached(string)$ void
  }
  class GameFlowEvents {
    <<static>>
    +event System.Action OnReturnToLandRequested$
    +RequestReturnToLand()$ void
  }
  class NotificationChannel {
    <<enum>>
    General
    Center
    System
    Resource
  }
  class NotificationManager {
    <<static>>
    +event Action~NotificationMessage~ OnNotify$
    +event Action~NotificationChannel~ OnClear$
    +Notify(string, NotificationChannel, Sprite, bool)$ void
    +Clear(NotificationChannel)$ void
  }
  class NotificationMessage {
    +string Text
    +Sprite Icon
    +NotificationChannel Channel
    +bool Sticky
    +NotificationMessage(string, NotificationChannel, Sprite, bool)
  }
  class NpcEvents {
    <<static>>
    +event System.Action~string~ OnNpcTalkedTo$
    +RaiseNpcTalkedTo(string)$ void
  }
  class RewardEvents {
    <<static>>
    +event System.Action~int~ OnMoneyRewarded$
    +RaiseMoneyRewarded(int)$ void
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
    +string CurrentPrompt$
    +bool IsRestricting$
    +Set(TutorialActionId, string)$ void
    +Clear()$ void
    +Blocks(TutorialActionId)$ bool
    +NotifyBlocked()$ void
  }
  NotificationMessage --> NotificationChannel : Channel
  TutorialGate --> TutorialActionId : RequiredAction
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CutsceneEvents | static | `CutsceneEvents.cs:6` | 8 |
| EnemyEvents | static | `EnemyEvents.cs:3` | 8 |
| ExploreEvents | static | `ExploreEvents.cs:5` | 8 |
| GameFlowEvents | static | `GameFlowEvents.cs:4` | 5 |
| NotificationChannel | enum | `NotificationChannel.cs:5` | 7 |
| NotificationManager | static | `NotificationManager.cs:7` | 12 |
| NotificationMessage | class | `NotificationMessage.cs:6` | 14 |
| NpcEvents | static | `NpcEvents.cs:4` | 8 |
| RewardEvents | static | `RewardEvents.cs:4` | 5 |
| TutorialActionId | enum | `TutorialEvents.cs:6` | 23 |
| TutorialEvents | static | `TutorialEvents.cs:36` | 8 |
| TutorialGate | static | `TutorialGate.cs:9` | 32 |
