# Core/Inputs/KeyBind/UIKeyBind

3 types · 143 LOC (non-blank) · depends on: [Core/Inputs/KeyBind](Core_Inputs_KeyBind.md) ×3, [Core/Inputs/KeyBind/MovePlayerDirsction](Core_Inputs_KeyBind_MovePlayerDirsction.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class KeyBindErrorUI {
    <<MB>>
    +Show(string) void
  }
  class KeyBindUI {
    <<MB>>
  }
  class MoveKeyBindUI {
    <<MB>>
  }
  KeyBindUI --> KeyBindErrorUI : errorUI
  MoveKeyBindUI --> KeyBindErrorUI : errorUI
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| KeyBindUI | KeyBindManager | OnBindingsChanged |
| MoveKeyBindUI | KeyBindManager | OnBindingsChanged |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| KeyBindErrorUI | MB | `KeyBindErrorUI.cs:7` | 26 |
| KeyBindUI | MB | `KeyBindUI.cs:8` | 58 |
| MoveKeyBindUI | MB | `MoveKeyBindUI.cs:6` | 59 |
