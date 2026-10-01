using Game.Core;
using Game.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class FishCollectionUI : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField] private Transform content;
        [SerializeField] private FishCollectionSlot slotPrefab;

        [Header("Category Buttons")]
        [SerializeField] private Button allButton;
        [SerializeField] private Button smallButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button largeButton;

        [Header("Panel")]
        [SerializeField] private GameObject collectionPanel;

        private readonly List<FishCollectionSlot> spawnedSlots = new();

        private CollectionFilter currentFilter = CollectionFilter.All;

        private enum CollectionFilter
        {
            All,
            Small,
            Medium,
            Large
        }

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            RefreshCollection();
        }

        private void SetupButtons()
        {
            if (allButton != null)
                allButton.onClick.AddListener(ShowAll);

            if (smallButton != null)
                smallButton.onClick.AddListener(ShowSmall);

            if (mediumButton != null)
                mediumButton.onClick.AddListener(ShowMedium);

            if (largeButton != null)
                largeButton.onClick.AddListener(ShowLarge);
        }

        public void RefreshCollection()
        {
            if (content == null)
            {
                Debug.LogWarning(
                    "[FishCollectionUI] Content is not assigned."
                );
                return;
            }

            if (FishCollectionManager.Instance == null)
            {
                Debug.LogWarning(
                    "[FishCollectionUI] FishCollectionManager not found."
                );
                return;
            }

            ClearSlots();

            IReadOnlyList<Item> fishItems =
                FishCollectionManager.Instance.GetCollectionItems();

            if (fishItems == null)
                return;

            for (int i = 0; i < fishItems.Count; i++)
            {
                Item fishItem = fishItems[i];

                if (fishItem == null)
                    continue;

                if (!PassesFilter(fishItem))
                    continue;

                FishCollectionSlot slot =
                    Instantiate(slotPrefab, content);

                bool collected =
                    FishCollectionManager.Instance.HasCollected(fishItem);

                slot.Setup(fishItem, collected);

                spawnedSlots.Add(slot);
            }
        }

        private bool PassesFilter(Item fishItem)
        {
            if (fishItem == null)
                return false;

            return currentFilter switch
            {
                CollectionFilter.All => true,

                CollectionFilter.Small =>
                    fishItem.FishSize == FishSize.Small,

                CollectionFilter.Medium =>
                    fishItem.FishSize == FishSize.Medium,

                CollectionFilter.Large =>
                    fishItem.FishSize == FishSize.Large,

                _ => true
            };
        }

        private void ClearSlots()
        {
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                if (spawnedSlots[i] != null)
                    Destroy(spawnedSlots[i].gameObject);
            }

            spawnedSlots.Clear();
        }

        public void ShowAll()
        {
            currentFilter = CollectionFilter.All;
            RefreshCollection();
        }

        public void ShowSmall()
        {
            currentFilter = CollectionFilter.Small;
            RefreshCollection();
        }

        public void ShowMedium()
        {
            currentFilter = CollectionFilter.Medium;
            RefreshCollection();
        }

        public void ShowLarge()
        {
            currentFilter = CollectionFilter.Large;
            RefreshCollection();
        }

        public void OpenCollection()
        {
            if (collectionPanel != null)
                collectionPanel.SetActive(true);

            RefreshCollection();
        }

        public void CloseCollection()
        {
            if (collectionPanel != null)
                collectionPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (allButton != null)
                allButton.onClick.RemoveListener(ShowAll);

            if (smallButton != null)
                smallButton.onClick.RemoveListener(ShowSmall);

            if (mediumButton != null)
                mediumButton.onClick.RemoveListener(ShowMedium);

            if (largeButton != null)
                largeButton.onClick.RemoveListener(ShowLarge);
        }
    }
}