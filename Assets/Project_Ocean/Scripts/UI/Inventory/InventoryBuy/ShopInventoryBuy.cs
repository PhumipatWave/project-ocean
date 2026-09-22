using UnityEngine;

namespace Game.Testing
{
    public class ShopInventoryBuy : MonoBehaviour
    {
        public static ShopInventoryBuy Instance { get; private set; }

        public ShopBuySlot[] shopbuySlots;

        public GameObject shopItemPrefab;
        public Item[] itemsToSell;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitializeShop();
        }

        void InitializeShop()
        {
            for (int i = 0; i < itemsToSell.Length; i++)
            {
                if (i < shopbuySlots.Length)
                {
                    // เปลี่ยนพารามิเตอร์ตรงนี้เป็น ShopSlot ด้วย
                    SpawnItemInShop(itemsToSell[i], shopbuySlots[i]);
                }
            }
        }

        // เปลี่ยน InventorySlot เป็น ShopSlot
        void SpawnItemInShop(Item item, ShopBuySlot slot)
        {
            GameObject newItemGo = Instantiate(shopItemPrefab, slot.transform);

            Draggableltem shopItem = newItemGo.GetComponent<Draggableltem>();
            shopItem.InitialiseItem(item);

            // This is a catalog listing (one entry per purchasable item), not a real stack —
            // RefreshCount()'s "1/99" reads as a held-item count, which is misleading here.
            if (shopItem.countText != null)
                shopItem.countText.gameObject.SetActive(false);

            shopItem.enabled = false;

            // ⭐ สำคัญมาก: ผูก item เข้ากับ slot
            slot.SetItem(shopItem);
        }
    }
}