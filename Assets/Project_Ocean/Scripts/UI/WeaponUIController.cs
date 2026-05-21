using TMPro;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Keeps Weapon HUD labels (WP_TextMH, WP_TextCP) in sync with inventory.
    /// Lives on the persistent GameHUD object.
    /// </summary>
    public class WeaponUIController : MonoBehaviour
    {
        public static WeaponUIController Instance { get; private set; }

        [SerializeField] private TMP_Text wpTextMH;
        [SerializeField] private TMP_Text wpTextCP;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            TryAutoBindTexts();
        }

        private void TryAutoBindTexts()
        {
            if (wpTextMH != null && wpTextCP != null)
                return;

            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (wpTextMH == null && text.gameObject.name == "WP_TextMH")
                    wpTextMH = text;
                if (wpTextCP == null && text.gameObject.name == "WP_TextCP")
                    wpTextCP = text;
            }
        }

        public void Refresh()
        {
            if (wpTextMH != null)
                wpTextMH.text = GetInventoryCount("MiniHub").ToString();

            if (wpTextCP != null)
                wpTextCP.text = GetInventoryCount("CheckPoint").ToString();
        }

        public static int GetInventoryCount(string itemId)
        {
            if (InventoryManager.Instance == null)
                return 0;

            int total = 0;
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                if (!slot.isUnlocked)
                    continue;

                Draggableltem itemInSlot = slot.GetComponentInChildren<Draggableltem>();
                if (itemInSlot != null && itemInSlot.item != null && itemInSlot.item.itemID == itemId)
                    total += itemInSlot.count;
            }

            return total;
        }
    }
}
