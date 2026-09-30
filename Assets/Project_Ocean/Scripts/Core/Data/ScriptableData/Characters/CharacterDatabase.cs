using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>One list of every CharacterData; assigned once on NPCManager.</summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Scriptable Objects/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private List<CharacterData> characters = new();

        private Dictionary<string, CharacterData> byId;

        public CharacterData Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            if (byId == null)
            {
                byId = new Dictionary<string, CharacterData>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in characters)
                    if (c != null && !string.IsNullOrEmpty(c.id))
                        byId[c.id] = c;
            }

            return byId.TryGetValue(id, out var data) ? data : null;
        }

        // Editing the list in the Inspector during play mode shouldn't need a domain reload.
        private void OnValidate() => byId = null;
    }
}
