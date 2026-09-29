using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class DraggableItem : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("UI")]
        public Image image;
        public TMP_Text countText;
        
        [HideInInspector] public Transform parentAfterDrag;
        private Transform slotBeforeDrag;
        [SerializeField] private Item item;
        public int Count { get; private set; } = 1;
        
        public void InitialiseItem(Item newItem, int newCount = 1)
        {
            item = newItem;
            image.sprite = newItem.ItemVisual;
            Count = Mathf.Max(1, newCount);
            RefreshCount();

        }// InitialiseItem ===// InitialiseItem === 

        public void RefreshCount()
        {
            int max = item != null ? item.MaxStackable : 1;
            countText.text = $"{Count}/{max}";
            bool textActive = max > 1; // single-stack items have nothing to show
            countText.gameObject.SetActive(textActive);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            slotBeforeDrag = transform.parent;
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root, true);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(parentAfterDrag);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            image.raycastTarget = true;
            InventoryManager.Instance?.HandleItemDropped(slotBeforeDrag, parentAfterDrag);
        }

        public Item GetItem() => item;

    }// DraggableItem ===
}
