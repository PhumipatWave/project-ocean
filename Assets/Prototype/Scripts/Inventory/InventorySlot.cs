using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour,IDropHandler
    {
        public Image image;
        public Color SelectedColor , notSelectedColor;

        private void Start()
        {
            Deselect();
        }

        public void Select()
        {
            image.color = SelectedColor;
        }

        public void Deselect()
        {
            image.color = notSelectedColor;
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0)
            {
                GameObject dropped =  eventData.pointerDrag;
                Draggableltem draggableltem = dropped.GetComponent<Draggableltem>();
                draggableltem.parentAfterDrag = transform;
                
            }
        }
        
    }
}
