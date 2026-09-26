# Gameplay/Contracts

4 types · 28 LOC (non-blank) · depends on: [Gameplay/QuestSystems/Quest](Gameplay_QuestSystems_Quest.md) ×1

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
| IDialogueView | interface | `IDialogueView.cs:6` | 8 |
| IModalPrompt | interface | `IModalPrompt.cs:4` | 6 |
| IQuestOfferView | interface | `IQuestOfferView.cs:4` | 5 |
| ModalPrompt | static | `IModalPrompt.cs:12` | 9 |
