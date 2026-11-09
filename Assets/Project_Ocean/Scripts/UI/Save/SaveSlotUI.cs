using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>One save-slot card: day, money and save time, or "Empty".</summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("Text fields")]
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text timeText;

        [Header("Buttons")]
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [Tooltip("Shown instead of Load/Delete when New Game found every slot full.")]
        [SerializeField] private Button selectButton;

        [Header("Empty slot state")]
        [SerializeField] private GameObject emptyStateLabel; // e.g. a "New Game" label, shown when meta == null

        private int _slot;
        private SaveMenuUI _menu;

        /// <summary>Populates this card. Pass meta = null for an empty/unused slot.</summary>
        public void Setup(int slot, SaveSlotMeta meta, SaveMenuUI menu, bool replaceMode)
        {
            _slot = slot;
            _menu = menu;

            bool hasSave = meta != null;
            bool replacing = replaceMode && hasSave;

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

            // Nothing to load in an empty slot; New Game fills empty slots itself.
            loadButton.gameObject.SetActive(!replacing);
            loadButton.interactable = hasSave;
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => _menu.SelectSlot(_slot, hasSave));

            if (selectButton != null)
            {
                selectButton.gameObject.SetActive(replacing);
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => _menu.SelectSlot(_slot, hasSave));
            }

            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(hasSave && !replacing);
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => _menu.DeleteSlot(_slot));
            }
        }
    }
}
