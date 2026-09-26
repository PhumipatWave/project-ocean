# Core/Inputs

1 types · 137 LOC (non-blank) · depends on: [Core/Inputs/KeyBind](Core_Inputs_KeyBind.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class InputReader {
    <<SO>>
    +Func~bool~ IsDialogueShowing$
    +event Action~bool~ OnAimed
    +event Action~bool~ OnAttacked
    +event Action~bool~ OnSprinted
    +event Action~bool~ OnInteracted
    +event Action~bool~ OnAmmoSelected
    +event Action~bool~ OnInventoryToggled
    +event Action OnMenuToggled
    +event Action~Vector2~ OnMoved
    +event Action OnUseItem
    ... +4 more fields
    +EnablePlayerInput() void
    +DisablePlayerInput() void
    +EnableUIInput() void
    +DisableUIInput() void
    +OnMove(InputAction.CallbackContext) void
    +OnMousePos(InputAction.CallbackContext) void
    +OnAim(InputAction.CallbackContext) void
    +OnAttack(InputAction.CallbackContext) void
    +OnSprint(InputAction.CallbackContext) void
    +OnInteract(InputAction.CallbackContext) void
    +TriggerInteract() void
    +OnItemConsume(InputAction.CallbackContext) void
    ... +9 more methods()
  }
```

## Event subscriptions

| Subscriber | Publisher | Event |
|---|---|---|
| InputReader | KeyBindManager | OnInitialized |

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| InputReader | SO | `InputReader.cs:9` | 137 |
