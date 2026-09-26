# Core/Events

9 types · 96 LOC (non-blank) · depends on: none

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
  NotificationMessage --> NotificationChannel : Channel
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CutsceneEvents | static | `CutsceneEvents.cs:6` | 8 |
| EnemyEvents | static | `EnemyEvents.cs:3` | 8 |
| ExploreEvents | static | `ExploreEvents.cs:5` | 8 |
| NotificationChannel | enum | `NotificationChannel.cs:5` | 7 |
| NotificationManager | static | `NotificationManager.cs:7` | 12 |
| NotificationMessage | class | `NotificationMessage.cs:6` | 14 |
| NpcEvents | static | `NpcEvents.cs:4` | 8 |
| TutorialActionId | enum | `TutorialEvents.cs:6` | 23 |
| TutorialEvents | static | `TutorialEvents.cs:36` | 8 |
