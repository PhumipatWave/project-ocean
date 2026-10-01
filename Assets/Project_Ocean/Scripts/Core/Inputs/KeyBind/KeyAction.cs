using UnityEngine;

namespace Game.Core
{
    // Explicit values: scenes store these as ints, and 3 (SwitchItem) was removed.
    public enum KeyAction
    {
        Move = 0,
        Interact = 1,
        Inventory = 2,
        UseItem = 4,
        Sprint = 5,
        Attack = 6,
        Aim = 7
    }

    /// <summary>Which device family a keybind row edits; values are the binding groups in the .inputactions.</summary>
    public enum ControlScheme
    {
        KeyboardMouse,
        Gamepad
    }

    public static class ControlSchemeExtensions
    {
        public static string Group(this ControlScheme scheme) =>
            scheme == ControlScheme.Gamepad ? "Gamepad" : "Keyboard&Mouse";
    }
}
