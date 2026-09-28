using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Who a dialogue line's characterId refers to — display name and portrait, keyed by id in
    /// CharacterDatabase. Lives outside any scene, so it resolves even when that character's
    /// NPC isn't loaded (or has no NPC at all, like "player").
    /// </summary>
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
