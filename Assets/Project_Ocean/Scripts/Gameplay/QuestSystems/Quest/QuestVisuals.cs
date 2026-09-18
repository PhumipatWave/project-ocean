using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>
    /// Shared sprite lookups/application for the quest UI panels (QuestSlot, QuestDetailPanel,
    /// QuestTrackerUI, ReviceQuestSlot) — was copy-pasted identically into each of them.
    /// </summary>
    public static class QuestVisuals
    {
        /// <summary>Resolves a quest item id (as authored on TargetQuestItemId — a stringified
        /// inventory item id) to that item's icon, or null if unset/unparseable/not found.</summary>
        public static Sprite GetItemSprite(string questItemId)
        {
            if (string.IsNullOrEmpty(questItemId)) return null;
            if (!int.TryParse(questItemId, out int id)) return null;
            return InventoryManager.Instance?.FindItemInDatabase(id)?.ItemVisual;
        }

        /// <summary>The shared "show it if we have art, hide it otherwise" pattern every quest
        /// panel applies to its item/avatar Image.</summary>
        public static void Apply(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
