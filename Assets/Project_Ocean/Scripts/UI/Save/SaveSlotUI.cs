using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>
    /// One card in the save-slot list (Stardew-style). Shows day/money/save-time
    /// for an occupied slot, or an "Empty" state for a slot with no save file.
    /// Attach to the slot prefab alongside its Text/Button children.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("Text fields")]
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text timeText;

        [Header("Buttons")]
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        [Header("Empty slot state")]
        [SerializeField] private GameObject emptyStateLabel; // e.g. a "New Game" label, shown when meta == null

        private int _slot;
        private SaveMenuUI _menu;

        /// <summary>Populates this card. Pass meta = null for an empty/unused slot.</summary>
        public void Setup(int slot, SaveSlotMeta meta, SaveMenuUI menu)
        {
            _slot = slot;
            _menu = menu;

            bool hasSave = meta != null;

            if (emptyStateLabel != null)
                emptyStateLabel.SetActive(!hasSave);

            if (dayText != null)   dayText.gameObject.SetActive(hasSave);
            if (moneyText != null) moneyText.gameObject.SetActive(hasSave);
            if (timeText != null)  timeText.gameObject.SetActive(hasSave);

            if (hasSave)
            {
                if (dayText != null)   dayText.text   = $"Day {meta.currentDay}";
                if (moneyText != null) moneyText.text = $"Money {meta.money}g";
                if (timeText != null)  timeText.text  = $"Save At {meta.saveTime}";
            }

            // Empty slots still load-able: LoadSlot(slot) on a fresh slot just
            // means "start playing here" — SaveGame(slot) is called on the
            // first real save from within the game itself.
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => _menu.SelectSlot(_slot, hasSave));

            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(hasSave);
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => _menu.DeleteSlot(_slot));
            }
        }
    }
}
