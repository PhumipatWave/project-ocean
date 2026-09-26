# UI/Dialogue

1 types · 19 LOC (non-blank) · depends on: [UI](UI.md) ×1, [Gameplay/Contracts](Gameplay_Contracts.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class DialoguePanelView {
    <<MB>>
    +GameObject Panel
    +bool IsOpen
    +SetOpen(bool) void
    +SetSpeaker(string, string, Sprite) void
    +SetText(string) void
  }
  class IDialogueView {
    <<Gameplay/Contracts>>
  }
  IDialogueView <|.. DialoguePanelView
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| DialoguePanelView | MB | `DialoguePanelView.cs:8` | 19 |
