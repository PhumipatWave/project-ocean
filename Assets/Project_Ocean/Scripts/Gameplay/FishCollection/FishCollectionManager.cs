using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Tracks which fish/items the player has discovered.
    /// The collection is based on ItemID and is independent from Inventory quantity.
    /// </summary>
    public class FishCollectionManager : Singleton<FishCollectionManager>, ISaveParticipant
    {
        [Header("Fish Collection Catalog")]
        [Tooltip("Items that are allowed to appear in the Fish Collection.")]
        [SerializeField] private List<Item> collectionItems = new();

        private readonly HashSet<int> collectedItemIds = new();

        public IReadOnlyCollection<int> CollectedItemIds => collectedItemIds;

        public int CollectedCount => collectedItemIds.Count;

        public int TotalFishCount => collectionItems != null
            ? collectionItems.Count
            : 0;

        // ─────────────────────────────────────────────
        // Collection
        // ─────────────────────────────────────────────

        /// <summary>
        /// Registers an item as discovered.
        /// Returns true only when this is a new discovery.
        /// </summary>
        public bool RegisterCatch(Item item)
        {
            if (item == null)
                return false;

            if (!IsCollectionItem(item))
                return false;

            bool isNew = collectedItemIds.Add(item.ItemID);

            if (isNew)
            {
                GameLog.Info(
                    $"[FishCollection] New discovery: {item.ItemName} (ID: {item.ItemID})"
                );

                if (CollectedCount >= TotalFishCount && TotalFishCount > 0)
                {
                    NotificationManager.Notify(
                        "FISH COLLECTION COMPLETE!",
                        NotificationChannel.Resource
                    );
                }
            }

            return isNew;
        }

        /// <summary>
        /// Checks whether this item has already been discovered.
        /// </summary>
        public bool HasCollected(Item item)
        {
            if (item == null)
                return false;

            return collectedItemIds.Contains(item.ItemID);
        }

        /// <summary>
        /// Checks collection status by Item ID.
        /// </summary>
        public bool HasCollected(int itemId)
        {
            return collectedItemIds.Contains(itemId);
        }

        /// <summary>
        /// Returns true when the item belongs to the Fish Collection catalog.
        /// </summary>
        public bool IsCollectionItem(Item item)
        {
            if (item == null || collectionItems == null)
                return false;

            for (int i = 0; i < collectionItems.Count; i++)
            {
                if (collectionItems[i] == null)
                    continue;

                if (collectionItems[i].ItemID == item.ItemID)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns all fish registered in the collection catalog.
        /// Useful later for FishCollectionUI.
        /// </summary>
        public IReadOnlyList<Item> GetCollectionItems()
        {
            return collectionItems;
        }

        /// <summary>
        /// Clears the collection for a New Game.
        /// </summary>
        public void ResetToDefault()
        {
            collectedItemIds.Clear();
        }

        // ─────────────────────────────────────────────
        // Save
        // ─────────────────────────────────────────────

        public void Collect(SaveData data)
        {
            if (data == null)
                return;

            data.collectedFishItemIds = new List<int>(collectedItemIds);
        }

        public void Apply(SaveData data)
        {
            collectedItemIds.Clear();

            if (data == null || data.collectedFishItemIds == null)
                return;

            for (int i = 0; i < data.collectedFishItemIds.Count; i++)
            {
                int itemId = data.collectedFishItemIds[i];

                // Only restore IDs that still exist in the current catalog.
                if (HasCatalogItem(itemId))
                    collectedItemIds.Add(itemId);
            }
        }

        private bool HasCatalogItem(int itemId)
        {
            if (collectionItems == null)
                return false;

            for (int i = 0; i < collectionItems.Count; i++)
            {
                if (collectionItems[i] != null &&
                    collectionItems[i].ItemID == itemId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}