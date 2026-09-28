using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
{
    public class SellSlot : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (transform.childCount == 0)
            {
                GameObject dropped = eventData.pointerDrag;
                DraggableItem item = dropped.GetComponent<DraggableItem>();

                item.parentAfterDrag = transform;
            }
        }
    }
}