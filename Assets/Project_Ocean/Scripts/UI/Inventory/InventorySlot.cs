using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour,IDropHandler
    {
        public Image image;
        public Color SelectedColor , notSelectedColor;
        
        // Lock Slot
        public bool isUnlocked = true;


        private void Awake()
        {
            Deselect();
        } // Awake ===

        public void Select()
        {
            image.color = SelectedColor;
        } // Select ===

        public void Deselect()
        {
            image.color = notSelectedColor;
            
        } // Deselect ===
        
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
    }
}
