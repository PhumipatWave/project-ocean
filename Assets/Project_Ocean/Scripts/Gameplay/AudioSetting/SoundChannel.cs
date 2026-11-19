using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>One audio channel; each child AudioSource is a sound, played by the child's name.</summary>
    public class SoundChannel : MonoBehaviour
    {
        private Dictionary<string, AudioSource> lookup;
        private readonly HashSet<string> warned = new HashSet<string>();

        public bool TryGet(string key, out AudioSource source)
        {
            if (lookup == null) Build();

            if (!string.IsNullOrEmpty(key) && lookup.TryGetValue(key, out source) && source.clip != null)
                return true;

            // Once per key — a missing sound must be visible, but not spam every call.
            if (warned.Add(key ?? "<null>"))
                Debug.LogWarning($"[SoundChannel] '{name}' has no sound named '{key}'.", this);

            source = null;
            return false;
        }

        private void Build()
        {
            lookup = new Dictionary<string, AudioSource>();

            foreach (var s in GetComponentsInChildren<AudioSource>(true))
            {
                if (s.transform == transform) continue; // the channel object itself is only a container
                if (!lookup.ContainsKey(s.name)) lookup.Add(s.name, s);
                else Debug.LogWarning($"[SoundChannel] '{name}' has two sounds named '{s.name}' — first one wins.", s);
            }
        }
    }
}
