using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        public Image image;
        public Color SelectedColor, notSelectedColor;
        public bool isSelected = false;

        // Lock Slot
        public bool isUnlocked = true;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            // ตรวจสอบว่าช่องนี้เป็นส่วนหนึ่งของหน้าต่าง ShopSystem หรือไม่
            ShopSystem shop = GetComponentInParent<ShopSystem>();

            if (shop != null)
            {
                // [แก้ปัญหาที่ 3] หากเป็นสล็อตฝั่งร้านค้า ให้เคลียร์สีสล็อตอื่นๆ ในร้านค้าทั้งหมดก่อน
                InventorySlot[] allShopSlots = shop.GetComponentsInChildren<InventorySlot>();
                foreach (var slot in allShopSlots)
                {
                    slot.Deselect();
                }

                // สลับมาเลือกและเปลี่ยนสีตัวเองเป็นสีเหลือง (SelectedColor)
                Select();

                // สั่งให้หน้าร้านค้าอัปเดตป้ายราคาชิ้นที่เลือกทันที
                shop.UpdateClickedItemPriceDisplay();
            }
            else
            {
                // ลอจิกเดิมสำหรับช่องกระเป๋าหลักตอนเดินเล่นในเกมปกติ
                if (InventoryManager.Instance != null)
                {
                    for (int i = 0; i < InventoryManager.Instance.InventorySlots.Length; i++)
                    {
                        if (InventoryManager.Instance.InventorySlots[i] == this)
                        {
                            InventoryManager.Instance.SendMessage("ChangeSelectedSlot", i, SendMessageOptions.DontRequireReceiver);
                            break;
                        }
                    }
                }
            }
        }

        public void Select()
        {
            image.color = SelectedColor;
            isSelected = true;
        } // Select ===

        public void Deselect()
        {
            image.color = notSelectedColor;
            isSelected = false;
        } // Deselect ===

        private void Awake()
        {
            image = GetComponent<Image>();
            Deselect();
            gameObject.SetActive(isUnlocked);
        } // Awake ===

        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0)
            {
                GameObject dropped = eventData.pointerDrag;
                Draggableltem draggableltem = dropped.GetComponent<Draggableltem>();
                draggableltem.parentAfterDrag = transform;
            }
        } // OnDrop ===

        public void SetLocked(bool locked)
        {
            isUnlocked = !locked;
            gameObject.SetActive(!locked);
        } // SetLocked ===
    }
}