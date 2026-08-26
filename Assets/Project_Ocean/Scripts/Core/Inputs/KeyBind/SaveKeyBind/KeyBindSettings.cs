using System;
using System.Collections.Generic;

namespace Game.Core
{
    [System.Serializable]
    public class KeyBindSettings
    {
        public List<KeyBindingData> bindings = new();
    }

    [System.Serializable]
    public class KeyBindingData
    {
        public string action;
        public List<string> bindingPaths = new();
    }
}