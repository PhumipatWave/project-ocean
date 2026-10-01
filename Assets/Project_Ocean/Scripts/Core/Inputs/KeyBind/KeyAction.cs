using UnityEngine;

namespace Game.Core
{
    // Keybind rows store these as ints: add new values at the end; removing or reordering means remapping the prefabs.
    public enum KeyAction
    {
        Move,
        Interact,
        Inventory,
        UseItem,
        Sprint,
        Attack,
        Aim
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
