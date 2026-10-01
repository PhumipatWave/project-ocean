using System;
using UnityEngine;

namespace Game.Core
{
    public class KeyBindUI : RebindButtonBase
    {
        [SerializeField] private KeyAction action;
        [SerializeField] private ControlScheme scheme = ControlScheme.KeyboardMouse;

        protected override void BeginRebind(KeyBindManager manager, Action<string, bool> onComplete) =>
            manager.StartRebind(action, scheme, onComplete);

        protected override string GetDisplayString(KeyBindManager manager) =>
            manager.GetDisplayString(action, scheme);
    }
}
