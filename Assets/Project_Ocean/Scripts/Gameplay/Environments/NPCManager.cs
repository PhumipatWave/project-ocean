using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Loaded NPCs by npcId; quests store only the id and look up the name and portrait here.</summary>
    public class NPCManager : Singleton<NPCManager>
    {

        [Tooltip("Name/portrait per characterId, independent of which scene's NPCs are loaded. Anything it doesn't cover falls back to the loaded NPC's own fields.")]
        [SerializeField] private CharacterDatabase characterDatabase;

        private readonly Dictionary<string, NPC> npcsById = new();

        protected override bool Persistent => true;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            // NPCs already in the scene may have tried to register before Instance existed.
            foreach (var npc in FindObjectsByType<NPC>(FindObjectsSortMode.None))
                Register(npc);
        }

        public void Register(NPC npc)
        {
            if (npc == null || string.IsNullOrEmpty(npc.NpcId))
                return;

            npcsById[npc.NpcId] = npc;
        }

        public void Unregister(NPC npc)
        {
            if (npc == null || string.IsNullOrEmpty(npc.NpcId))
                return;

            // Only remove if this instance is still the one registered — avoids a late
            // OnDisable clobbering a newer registration for the same id.
            if (npcsById.TryGetValue(npc.NpcId, out var current) && current == npc)
                npcsById.Remove(npc.NpcId);
        }

        public NPC GetNpc(string npcId) =>
            !string.IsNullOrEmpty(npcId) && npcsById.TryGetValue(npcId, out var npc) ? npc : null;

        public string GetNpcName(string npcId)
        {
            var name = characterDatabase != null ? characterDatabase.Get(npcId)?.displayName : null;
            return string.IsNullOrEmpty(name) ? GetNpc(npcId)?.NpcName : name;
        }

        public Sprite GetNpcAvatar(string npcId)
        {
            var portrait = characterDatabase != null ? characterDatabase.Get(npcId)?.portrait : null;
            return portrait != null ? portrait : GetNpc(npcId)?.NpcPortrait;
        }
    }
}
