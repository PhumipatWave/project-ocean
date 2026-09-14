using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Registry of all currently-loaded NPC instances, keyed by npcId. NPCs
    /// self-register/unregister (OnEnable/OnDisable) rather than being scanned via
    /// FindObjectsOfType, so the registry stays correct across additive scene loads
    /// and unloads (e.g. Land_Map loading/unloading).
    ///
    /// Lets other systems (quest board/tracker UI) resolve an NPC's name/avatar from
    /// just its id, instead of duplicating that data onto the quest itself — quest data
    /// only ever stores npcId; this is what "syncs" it.
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }

        private readonly Dictionary<string, NPC> npcsById = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        public string GetNpcName(string npcId) => GetNpc(npcId)?.NpcName;

        public Sprite GetNpcAvatar(string npcId) => GetNpc(npcId)?.NpcPortrait;
    }
}
