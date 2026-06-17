using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Testing
{
    public class SellSlot : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0)
            {
                GameObject dropped = eventData.pointerDrag;
                Draggableltem item = dropped.GetComponent<Draggableltem>();

                item.parentAfterDrag = transform;
            }
        }
    }
}