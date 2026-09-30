using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
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

            // Awake/OnEnable order between sibling scene objects isn't guaranteed — an NPC
            // already in the scene when this Awake runs may have already tried (and silently
            // no-op'd) to register itself before Instance existed here. Confirmed this was
            // actually happening (registry stayed empty at runtime) while wiring Cutscene's
            // NPC-binding lookup — same fix already applied to CutsceneManager.
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
