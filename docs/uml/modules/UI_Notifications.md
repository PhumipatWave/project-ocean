# UI/Notifications

5 types · 180 LOC (non-blank) · depends on: [Core/Events](Core_Events.md) ×8, [Core/Data/Items](Core_Data_Items.md) ×1, [Gameplay/Entities/Players](Gameplay_Entities_Players.md) ×1, [UI/Inventory](UI_Inventory.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class NotificationLine {
    <<MB>>
    +Setup(NotificationMessage) void
  }
  class NotificationView {
    <<MB>>
    <<abstract>>
    #OnEnable() void
    #OnDisable() void
    #Show(NotificationMessage)* void
    #Clear() void
  }
  class ResourceNotificationPresenter {
    <<MB>>
    +Format(int, string)$ string
  }
  class StackNotificationView {
    <<MB>>
    #OnDisable() void
    #Show(NotificationMessage) void
  }
  class ToastNotificationView {
    <<MB>>
    #OnDisable() void
    #Show(NotificationMessage) void
    #Clear() void
  }
  NotificationView <|-- StackNotificationView
  NotificationView <|-- ToastNotificationView
  StackNotificationView --> NotificationLine : linePrefab
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| NotificationView | NotificationManager | OnClear |
| NotificationView | NotificationManager | OnNotify |
| ResourceNotificationPresenter | InventoryManager | OnItemDelta |
| ResourceNotificationPresenter | PlayerMoney | OnMoneyDelta |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| NotificationLine | MB | `NotificationLine.cs:12` | 32 |
| NotificationView | MB, abstract | `NotificationView.cs:8` | 26 |
| ResourceNotificationPresenter | MB | `ResourceNotificationPresenter.cs:8` | 25 |
| StackNotificationView | MB | `StackNotificationView.cs:8` | 22 |
| ToastNotificationView | MB | `ToastNotificationView.cs:11` | 75 |
