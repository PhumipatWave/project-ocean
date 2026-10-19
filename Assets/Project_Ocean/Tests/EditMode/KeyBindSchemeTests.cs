using Game.Core;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace Game.Tests
{
    public class KeyBindSchemeTests
    {
        [Test]
        public void GamepadRebind_SurvivesSaveLoad_AndLeavesKeyboardAlone()
        {
            var actions = new InputSystem_Actions();
            var interact = actions.Player.Interact;

            int kb = KeyBindManager.GetBindingIndex(interact, ControlScheme.KeyboardMouse);
            int pad = KeyBindManager.GetBindingIndex(interact, ControlScheme.Gamepad);
            Assert.AreEqual("<Keyboard>/e", interact.bindings[kb].effectivePath);
            Assert.AreEqual("<Gamepad>/buttonSouth", interact.bindings[pad].effectivePath);

            interact.ApplyBindingOverride(pad, "<Gamepad>/buttonNorth");
            string json = actions.asset.SaveBindingOverridesAsJson();
            UnityEngine.Object.DestroyImmediate(actions.asset);

            var reloaded = new InputSystem_Actions();
            reloaded.asset.LoadBindingOverridesFromJson(json);
            var again = reloaded.Player.Interact;
            Assert.AreEqual("<Gamepad>/buttonNorth", again.bindings[KeyBindManager.GetBindingIndex(again, ControlScheme.Gamepad)].effectivePath);
            Assert.AreEqual("<Keyboard>/e", again.bindings[KeyBindManager.GetBindingIndex(again, ControlScheme.KeyboardMouse)].effectivePath);
            UnityEngine.Object.DestroyImmediate(reloaded.asset);
        }
    }
}
