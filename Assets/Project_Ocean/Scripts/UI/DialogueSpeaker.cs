using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Fills a dialogue panel's speaker name and portrait from a characterId, else the caller's fallback.</summary>
    public static class DialogueSpeaker
    {
        public static void Apply(TMP_Text nameText, Image avatar, string characterId,
            string fallbackName = null, Sprite fallbackPortrait = null)
        {
            var manager = NPCManager.Instance;

            var name = manager?.GetNpcName(characterId);
            if (string.IsNullOrEmpty(name)) name = fallbackName;

            var portrait = manager?.GetNpcAvatar(characterId);
            if (portrait == null) portrait = fallbackPortrait;

            if (nameText != null) nameText.text = name ?? "";

            if (avatar != null)
            {
                avatar.gameObject.SetActive(portrait != null);
                if (portrait != null) avatar.sprite = portrait;
            }
        }
    }
}
