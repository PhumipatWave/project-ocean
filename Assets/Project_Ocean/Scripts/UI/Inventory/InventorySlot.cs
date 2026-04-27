using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour,IDropHandler, IPointerClickHandler
    {
        public Image image;
        public Color SelectedColor , notSelectedColor;
        public bool isSelected = false;
        
        // Lock Slot
        public bool isUnlocked = true;

        public void OnPointerClick(PointerEventData eventData)
        {
            // สั่งให้ InventoryManager เปลี่ยนมาเลือกช่องนี้
            // โดยการ Deselect ช่องอื่นทั้งหมดก่อน แล้วค่อย Select ช่องนี้
            foreach (var slot in InventoryManager.Instance.inventorySlots)
            {
                slot.Deselect();
            }
            Select();
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
