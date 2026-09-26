# Core/Inputs/KeyBind/SaveKeyBind

3 types · 28 LOC (non-blank) · depends on: [Core/Inputs/KeyBind](Core_Inputs_KeyBind.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class KeyBindResetAllButton {
    <<MB>>
  }
  class KeyBindSettings {
    +List~KeyBindingData~ bindings
  }
  class KeyBindingData {
    +string action
    +List~string~ bindingPaths
  }
  KeyBindSettings --> KeyBindingData : bindings
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| KeyBindResetAllButton | MB | `KeyBindResetAllButton.cs:6` | 19 |
| KeyBindSettings | class | `KeyBindSettings.cs:7` | 4 |
| KeyBindingData | class | `KeyBindSettings.cs:13` | 5 |
