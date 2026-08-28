using System;
using System.Collections.Generic;

namespace Game.Core
{
    [Serializable]
    public class KeyBindSettings
    {
        public List<KeyBindingData> bindings = new();
    }

    [Serializable]
    public class KeyBindingData
    {
        public string action;
        public List<string> bindingPaths = new();
    }
}