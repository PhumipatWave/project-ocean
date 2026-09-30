using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
    public class KeyBindUI : RebindButtonBase
    {
        [SerializeField] private KeyAction action;

        protected override void BeginRebind(KeyBindManager manager, Action<string, bool> onComplete) =>
            manager.StartRebind(action, onComplete);

        protected override string GetDisplayString(KeyBindManager manager)
        {
            var inputAction = manager.GetAction(action);
            return inputAction != null
                ? inputAction.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions)
                : "---";
        }
    }
}
