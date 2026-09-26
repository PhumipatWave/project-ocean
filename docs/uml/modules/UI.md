# UI

16 types · 720 LOC (non-blank) · depends on: [Core](Core.md) ×12, [Core/Events](Core_Events.md) ×4, [Core/Save](Core_Save.md) ×3, [Core/Save/DayReset](Core_Save_DayReset.md) ×3, [Core/Inputs](Core_Inputs.md) ×2, [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×2, [Core/Services](Core_Services.md) ×2, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1, [Core/Datas](Core_Datas.md) ×1, [UI/Upgrades](UI_Upgrades.md) ×1, [Gameplay/Environments](Gameplay_Environments.md) ×1, [Gameplay/Entities/Enemies](Gameplay_Entities_Enemies.md) ×1, [UI/UIManager](UI_UIManager.md) ×1, [Gameplay/Entities/Players/OxygenTube](Gameplay_Entities_Players_OxygenTube.md) ×1, [Gameplay/Cutscenes](Gameplay_Cutscenes.md) ×1, [UI/UIMenuSetting](UI_UIMenuSetting.md) ×1, [UI/Loading](UI_Loading.md) ×1, [UI/Inventory](UI_Inventory.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class AmmoWheel {
    <<MB>>
    +AmmoType currentAmmoType
    +int AmmoIndex
  }
  class ButtonHoverScale {
    <<MB>>
    +OnPointerEnter(PointerEventData) void
    +OnPointerExit(PointerEventData) void
    +ResetButton() void
    +RefreshPrice() void
  }
  class ConfirmationDialog {
    <<MB>>
    +ConfirmationDialog Instance$
    +Show(string, Action, Action) void
    +Confirm() void
    +Cancel() void
  }
  class DialogueSpeaker {
    <<static>>
    +Apply(TMP_Text, Image, string, string, Sprite)$ void
  }
  class EnemyHealthDisplay {
    <<MB>>
  }
  class EscClosablePanel {
    <<MB>>
  }
  class GameHUD {
    <<MB>>
    +GameHUD instance$
    +FadeIn() void
  }
  class LandUI {
    <<MB>>
    +OpenUI() void
    +CloseUI() void
  }
  class MainMenu {
    <<MB>>
    +StartGame() void
    +QuiteGame() void
  }
  class NextDayAnimationUI {
    <<MB>>
    +bool IsPlaying$
  }
  class NotificationUI {
    <<MB>>
  }
  class PunchScale {
    <<MB>>
    +Play() void
    +Evaluate(float, float)$ float
  }
  class SaveNotificationUI {
    <<MB>>
  }
  class TipPanelUI {
    <<MB>>
  }
  class UIFadeBackground {
    <<MB>>
    +Show() void
    +Hide() void
    +SetDim(float) void
  }
  class UILock {
    <<MB>>
  }
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| AmmoWheel | InputReader | OnAmmoSelected |
| GameHUD | DayManager | OnDayAdvanced |
| GameHUD | InputReader | OnMenuToggled |
| GameHUD | PlayerUIState | OnLockChanged |
| LandUI | DayManager | OnDayAdvanced |
| LandUI | DayManager | OnDivedToday |
| LandUI | DialogueState | OnChanged |
| LandUI | FeatureUnlockService | OnFeatureUnlocked |
| LandUI | LoadingManager | OnLoadingFinished |
| NextDayAnimationUI | DayManager | OnDayAdvanced |
| SaveNotificationUI | SaveBridge | OnSaved |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| AmmoWheel | MB | `AmmoWheel.cs:5` | 50 |
| ButtonHoverScale | MB | `ButtonHoverScale.cs:6` | 59 |
| ConfirmationDialog | MB | `ConfirmationDialog.cs:10` | 56 |
| DialogueSpeaker | static | `DialogueSpeaker.cs:13` | 18 |
| EnemyHealthDisplay | MB | `EnemyHealthDisplay.cs:6` | 9 |
| EscClosablePanel | MB | `EscClosablePanel.cs:11` | 11 |
| GameHUD | MB | `GameHUD.cs:11` | 186 |
| LandUI | MB | `LandUI.cs:10` | 142 |
| MainMenu | MB | `MainMenu.cs:6` | 11 |
| NextDayAnimationUI | MB | `NextDayAnimationUI.cs:8` | 48 |
| NotificationUI | MB | `NotificationUI.cs:6` | 12 |
| PunchScale | MB | `PunchScale.cs:7` | 30 |
| SaveNotificationUI | MB | `SaveNotificationUI.cs:6` | 7 |
| TipPanelUI | MB | `TipPanelUI.cs:6` | 34 |
| UIFadeBackground | MB | `UIFadeBackground.cs:5` | 40 |
| UILock | MB | `UILock.cs:5` | 7 |
