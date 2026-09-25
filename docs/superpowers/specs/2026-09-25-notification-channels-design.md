# Notification Channels — Design

Date: 2026-09-25
Status: Approved

## Goal

Split the single top-of-screen notification into four screen areas, each with its own
behaviour, using a small OOP structure (message object + event bus + view classes).

| Channel    | Position     | Behaviour                                   | Messages                                   |
|------------|--------------|---------------------------------------------|--------------------------------------------|
| `General`  | top center   | one toast at a time, fades, one pending slot | locks, "Not enough money", "Inventory is full", dive limit, save-slot warnings |
| `Center`   | screen center| one toast at a time, fades, one pending slot | quest step text, tutorial prompt           |
| `System`   | bottom right | one toast at a time, fades, one pending slot | "Game Saved!"                              |
| `Resource` | bottom left  | stack of up to 4 lines, each fades alone     | `+50 Money`, `-20 Money`, `+1 Tuna`, `-1 Bait` (with icon) |

The cutscene skip hint is not a notification. It keeps its own `CutsceneSkipHint` object and
only moves to the bottom right, below `System` toasts.

## Decisions

- Tutorial prompts fade like any toast. The top-left tutorial detail panel already keeps the
  instruction on screen, so the persistent-message API is removed.
- Resource lines stack (max 4, oldest removed first) and show an icon: the item's
  `Item.ItemVisual`, or a coin sprite for money.
- Resource triggers: money gain and spend, loot and pickups, shop buy and sell, items consumed
  or handed in to quests. Save load and new-game reset stay silent.

## Classes

### Data

- `NotificationChannel` — enum: `General`, `Center`, `System`, `Resource`.
- `NotificationMessage` — immutable class: `Text` (string), `Icon` (Sprite, optional),
  `Channel` (NotificationChannel).

### Bus — `NotificationManager` (existing static class, `UI/NotificationManager.cs`)

- `static event Action<NotificationMessage> OnNotify`
- `static void Notify(string text, NotificationChannel channel = NotificationChannel.General, Sprite icon = null)`
  — ignores null/empty text. Existing one-argument calls keep compiling and route to `General`.
- Removed: `ShowPersistent`, `ClearPersistent`, `PulsePersistent`, `OnPersistentChanged`,
  `OnPersistentPulse`.

### Views (MonoBehaviours)

- `NotificationView` (abstract)
  - `[SerializeField] NotificationChannel channel`
  - Subscribes to `NotificationManager.OnNotify` in `OnEnable`, unsubscribes in `OnDisable`.
  - Forwards only messages whose `Channel` matches to `protected abstract void Show(NotificationMessage msg)`.
- `ToastNotificationView : NotificationView` — replaces `NotificationTextUI`.
  - Fields: `CanvasGroup canvasGroup`, `TextMeshProUGUI messageText`, optional `Image icon`,
    `showDuration = 2`, `fadeDuration = 0.4`.
  - Fade in, hold, fade out, using unscaled time.
  - While showing: the same text is ignored; a different text replaces the single pending
    slot (latest wins) and plays after the current toast ends.
  - `OnDisable`: stop the coroutine, set alpha to 0, clear current and pending state.
- `StackNotificationView : NotificationView`
  - Fields: `NotificationLine linePrefab`, `Transform container` (has a VerticalLayoutGroup,
    lower-left aligned), `maxLines = 4`.
  - `Show` instantiates a line and calls `Setup(msg)`. If the container already holds
    `maxLines` lines, the oldest is destroyed first.
- `NotificationLine` (on the line prefab)
  - Fields: `CanvasGroup`, `Image icon`, `TextMeshProUGUI text`, `showDuration`, `fadeDuration`.
  - `Setup(msg)` sets text and icon (icon GameObject hidden when `msg.Icon` is null), fades in,
    holds, fades out with unscaled time, then destroys itself.

### Presenter

- `ResourceNotificationPresenter` (MonoBehaviour)
  - `[SerializeField] Sprite moneyIcon` (may be empty — line shows text only).
  - Subscribes to `PlayerMoney.OnMoneyDelta(int delta)` and
    `InventoryManager.OnItemDelta(Item item, int delta)`; unsubscribes in `OnDisable`.
  - `public static string Format(int delta, string name)` → `"+1 Tuna"`, `"-20 Money"`.
  - Ignores `delta == 0` and null items.
  - Calls `NotificationManager.Notify(Format(...), NotificationChannel.Resource, icon)`.

### Gameplay changes

- `PlayerMoney`
  - New `public static event Action<int> OnMoneyDelta`, raised by `AddMoney(amount)` with
    `+amount` and by a successful `Use(amount)` with `-amount`.
  - `AddMoneySilent` does not raise it. `OnMoneyChanged(total)` is unchanged.
  - Its own `Notify("+X Money")` is removed.
- `InventoryManager`
  - New `public static event Action<Item, int> OnItemDelta` and one public
    `RaiseItemDelta(Item item, int delta)` (public only so `ShopTransactionService` can use it).
  - Raised from `AddItem` (+1 on success), `ConsumeItem` (-1 on success), `RemoveQuestItem`
    (-count on success), `AddRewardItem` (+1 on success), `RemoveSelectedItem` (-removed count).
  - Shop sell removal (`ShopTransactionService`) calls `inventory.RaiseItemDelta(item, -1)`.
    `SellAll` calls it once per item type with the total count. Shop buy already goes through
    `AddItem`, so it needs no extra call.
  - `ResetToDefault()` and `SaveBridge.ApplyInventory` never raise it.
- `QuestManager` — the `Notify($"+{RewardMoney} Money")` on step reward is removed (the
  presenter shows it via `AddMoney`). Step-description toast goes to `Center`.
- `TutorialPromptUI`
  - `Show` calls `Notify(text, Center, image)` only when the text differs from
    `lastShownText`. `Hide` resets `lastShownText` and `TutorialGate.RequiredAction`; it no
    longer clears any toast.
  - Exposes the current prompt text so `TutorialEvents.NotifyBlocked()` can re-notify it on
    `Center` (replaces `PulsePersistent`).
- `SaveNotificationUI` — notifies on `System`.
- All other existing `Notify(text)` callers are unchanged and land on `General`.

## Data flow

```
gameplay code ──Notify(text, channel, icon)──► NotificationManager.OnNotify(msg)
                                                   │
               ┌──────────────┬──────────────┬─────┴────────┐
          Toast(General)  Toast(Center)  Toast(System)  Stack(Resource)

PlayerMoney.OnMoneyDelta ───────┐
InventoryManager.OnItemDelta ───┴─► ResourceNotificationPresenter ──Notify(..., Resource, icon)
```

Every view receives every message and ignores other channels. No code calls a view directly.

## Scene and prefabs

Current `AS_PlayerHUD`:

```
GameHUD                 (GameHUD, root canvas)
├── Notification        NotificationTextUI + SaveNotificationUI
├── ConfirmDialog
└── SkipText            CutsceneSkipHint
```

New prefabs:

- `Assets/Project_Ocean/Prefabs/UI/Notifications/NotificationLayer.prefab` — full-screen
  RectTransform, no raycast blocking.
  ```
  NotificationLayer    SaveNotificationUI, ResourceNotificationPresenter
  ├── Toast_General    top center     ToastNotificationView (General)
  ├── Toast_Center     screen center  ToastNotificationView (Center)
  ├── Toast_System     bottom right   ToastNotificationView (System), above SkipText
  └── Stack_Resource   bottom left    StackNotificationView (Resource) + VerticalLayoutGroup
  ```
- `Assets/Project_Ocean/Prefabs/UI/Notifications/NotificationLine.prefab`
  ```
  NotificationLine   CanvasGroup, HorizontalLayoutGroup, NotificationLine
  ├── Icon           Image (preserveAspect, ~32x32)
  └── Text           TextMeshProUGUI
  ```

Scene edits (Unity MCP, `AS_PlayerHUD` only):

- Place `NotificationLayer` under `GameHUD`, outside GameHUD's hideable CanvasGroup (same
  level as the current `Notification`), so notifications still show during cutscenes.
- Copy the old `Notification` font/size/color to `Toast_General`, then delete `Notification`.
- Move `SkipText` to the bottom right.
- After saving, confirm the scene path with `git status` (known MCP save-path bug).

Inspector values: `moneyIcon` = coin sprite (optional), `linePrefab` = `NotificationLine`,
`maxLines` = 4, `showDuration` = 2, `fadeDuration` = 0.4.

## Files

Paths are relative to `Assets/Project_Ocean/`. New code follows the CLAUDE.md comment rules
(short, why-only).

New:

- `Scripts/UI/Notifications/NotificationChannel.cs`
- `Scripts/UI/Notifications/NotificationMessage.cs`
- `Scripts/UI/Notifications/NotificationView.cs`
- `Scripts/UI/Notifications/ToastNotificationView.cs`
- `Scripts/UI/Notifications/StackNotificationView.cs`
- `Scripts/UI/Notifications/NotificationLine.cs`
- `Scripts/UI/Notifications/ResourceNotificationPresenter.cs`
- `Tests/EditMode/Game.Tests.EditMode.asmdef`, `Tests/EditMode/NotificationTests.cs`
- The two prefabs above

Changed: `NotificationManager.cs`, `SaveNotificationUI.cs`, `PlayerMoney.cs`,
`InventoryManager.cs`, `ShopTransactionService.cs`, `QuestManager.cs`, `TutorialPromptUI.cs`,
`TutorialEvents.cs`, `AS_PlayerHUD.unity`.

Deleted: `NotificationTextUI.cs` (+ `.meta`).

## Edge cases

| Case | Handling |
|------|----------|
| `timeScale = 0` (tutorial UseItem pause, pause menu) | All fades use unscaled time. |
| View disabled mid-fade | `OnDisable` resets state so the view never stays stuck. |
| Same text re-sent while a toast shows | Ignored by toast views. Stack view shows every change. |
| Stack full | Oldest line destroyed immediately. |
| Sell All | One item delta per item type plus one money delta. |
| Shop buy | Two lines: `-X Money`, `+1 Item`. |
| `delta == 0`, failed `Use()`, null item | No notification. |
| No icon | Icon object hidden, text only. |
| Save load / new game reset | Silent paths only (`AddMoneySilent`, direct slot writes, `ResetToDefault`). |
| Quest money reward | Shown once, by the presenter. |
| Main menu | Views live from the menu on; no gameplay events fire there. |
| Static events | Every subscriber unsubscribes in `OnDisable`. |

## Testing

- EditMode tests (`NotificationTests.cs`):
  - `Notify(text)` reaches subscribers with `Channel == General`.
  - `Notify(text, Resource, icon)` keeps channel and icon.
  - `ResourceNotificationPresenter.Format(1, "Tuna") == "+1 Tuna"` and
    `Format(-20, "Money") == "-20 Money"`.
- Play mode via Unity MCP (set `Application.runInBackground = true`, verify live):
  land after a dive, buy, Sell All, ocean pickup, quest step advance, tutorial step plus a
  blocked key, cutscene with skip hint.
- Console clean after compile; run `graphify update .` at the end.

## Out of scope

- Merging repeated resource lines (e.g. three `+1 Tuna` into `+3 Tuna`).
- Per-channel sounds.
- Localization of notification text.
