using UnityEngine.InputSystem;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
{
    [System.Serializable]
    public class KeyBind
    {
        public KeyAction Action;
        public InputBinding Binding;

        public KeyBind(KeyAction action, InputBinding binding)
        {
            Action = action;
            Binding = binding;
        }
    }
}