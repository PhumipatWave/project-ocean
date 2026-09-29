# Core/Inputs/KeyBind

2 types · 350 LOC (non-blank) · depends on: [Core/Inputs/KeyBind/SaveKeyBind](Core_Inputs_KeyBind_SaveKeyBind.md) ×2, [Core](Core.md) ×1, [Core/Inputs/KeyBind/MoveDirection](Core_Inputs_KeyBind_MoveDirection.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class KeyAction {
    <<enum>>
    Move
    Interact
    Inventory
    SwitchItem
    UseItem
    Sprint
    Attack
    Aim
  }
  class KeyBindManager {
    <<MB>>
    +KeyBindManager Instance$
    +event Action OnInitialized$
    +event Action OnBindingsChanged$
    +InputSystem_Actions Actions
    +IsBindingUsed(string, KeyAction, int) bool
    +GetAction(KeyAction) InputAction
    +StartRebind(KeyAction, Action~string, bool~) void
    +GetMoveBinding(MoveDirection) InputAction
    +GetMoveBindingIndex(MoveDirection) int
    +GetMoveBindingDisplayString(MoveDirection) string
    +RebindMove(MoveDirection, Action~string, bool~) void
    +SaveBindings() void
    +LoadBindings() void
    +ResetAllToDefault() void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| KeyAction | enum | `KeyAction.cs:5` | 11 |
| KeyBindManager | MB | `KeyBindManager.cs:8` | 339 |
