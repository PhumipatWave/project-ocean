using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        public Image image;
        public Color SelectedColor , notSelectedColor;
        public bool isSelected = false;
        
        // Lock Slot
        public bool isUnlocked = true;

        public void OnPointerClick(PointerEventData eventData)
        {
            // ตรวจสอบก่อนว่าช่องนี้ปลดล็อกแล้วถึงจะเลือกได้
            if (!isUnlocked) return;

            // วนลูปหาว่าช่องนี้เป็นสมาชิกลำดับที่เท่าไหร่ใน InventoryManager
            for (int i = 0; i < InventoryManager.Instance.inventorySlots.Length; i++)
            {
                if (InventoryManager.Instance.inventorySlots[i] == this)
                {
                    // เรียกผ่าน InventoryManager เพื่อให้มันจัดการสลับสี และอัปเดต selectedSlot ให้ถูกต้อง
                    InventoryManager.Instance.SendMessage("ChangeSelectedSlot", i, SendMessageOptions.DontRequireReceiver);
                    break;
                }
            }
        }

        public void Select()
        {
            image.color = SelectedColor;
        } // Select ===

        public void Deselect()
        {
            image.color = notSelectedColor;

        } // Deselect ===

        private void Awake()
        {
            Deselect();
            image = GetComponent<Image>();

            // เพิ่มบรรทัดนี้เพื่อให้มันซ่อนตัวเองตามสถานะเริ่มต้นที่ตั้งไว้ใน Inspector
            gameObject.SetActive(isUnlocked);
        } // Awake ===

        
        
        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0)
            {
                GameObject dropped =  eventData.pointerDrag;
                Draggableltem draggableltem = dropped.GetComponent<Draggableltem>();
                draggableltem.parentAfterDrag = transform;
                
            }
        } // OnDrop ===
        
        public void SetLocked(bool locked)
        {
            isUnlocked = !locked;

            gameObject.SetActive(!locked); // ซ่อนทั้งช่อง
            
        } // SetLocked ===

        /*public void OnClickSlot()
        {
            isSelected = !isSelected;

            image.color = isSelected ? Color.yellow : Color.white;
        }*/

        
    }
}
