using UnityEngine;

namespace Game.Testing
{
    public class ShopInventoryBuy : MonoBehaviour
    {
        public static ShopInventoryBuy Instance { get; private set; }

        // เปลี่ยนจาก InventorySlot[] เป็น ShopSlot[] เพื่อให้ลาก Slot ใน Shop ใส่ได้
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

            // ปรับแต่ง: อาจจะปิดการลากในหน้า Shop เพื่อไม่ให้คนเล่นลากของออกจากร้านฟรีๆ
            shopItem.enabled = false;
        }
    }
}