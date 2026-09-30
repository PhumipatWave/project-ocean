# Gameplay/Contracts

5 types · 34 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class IDialogueView {
    <<interface>>
    +GameObject Panel
    +bool IsOpen
    +SetOpen(bool)* void
    +SetSpeaker(string, string, Sprite)* void
    +SetText(string)* void
  }
  class IInteractPopupView {
    <<interface>>
    +bool IsOpen
    +Show(string, string, Sprite, Sprite)* void
    +Hide()* void
  }
  class IModalPrompt {
    <<interface>>
    +bool CanShow
    +Show(string, System.Action, System.Action)* void
    +Cancel()* void
  }
  class ModalPrompt {
    <<static>>
    +IModalPrompt Current$
    +Register(IModalPrompt)$ void
    +Unregister(IModalPrompt)$ void
  }
  class IQuestOfferView {
    <<interface>>
    +ShowPanel(Quest, System.Action)* void
    +HidePanel()* void
  }
  ModalPrompt --> IModalPrompt : Current
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| IDialogueView | interface | `IDialogueView.cs:7` | 8 |
| IInteractPopupView | interface | `IInteractPopupView.cs:6` | 6 |
| IModalPrompt | interface | `IModalPrompt.cs:6` | 6 |
| IQuestOfferView | interface | `IQuestOfferView.cs:6` | 5 |
| ModalPrompt | static | `IModalPrompt.cs:14` | 9 |
