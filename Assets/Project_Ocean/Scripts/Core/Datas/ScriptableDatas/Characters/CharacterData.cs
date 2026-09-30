using UnityEngine;

namespace Game.Core
{
    /// <summary>Name and portrait for a dialogue characterId; works even when that character's NPC isn't loaded.</summary>
    [CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
    public class CharacterData : ScriptableObject
    {
        [Tooltip("Matches a dialogue line's Character Id (and an NPC's Npc Id).")]
        public string id;
        public string displayName;
        [Tooltip("Optional — leave empty for a speaker with no portrait (e.g. the player).")]
        public Sprite portrait;
    }
}
