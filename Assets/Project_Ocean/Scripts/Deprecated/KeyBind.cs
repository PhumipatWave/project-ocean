using UnityEngine.InputSystem;

namespace Game.Core
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