using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Game.Testing
{
    public class DraggableItem : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("UI")]
        public Image image;
        public TMP_Text countText;
        
        [HideInInspector] public Transform parentAfterDrag;
        [SerializeField] private Item item;
        [HideInInspector] public int count = 1;
        
        public void InitialiseItem(Item newItem, int newCount = 1)
        {
            item = newItem;
            image.sprite = newItem.ItemVisual;
            count = Mathf.Max(1, newCount);
            RefreshCount();

        }// InitialiseItem ===// InitialiseItem === 

        public void RefreshCount()
        {
            int max = item != null ? item.MaxStackable : 1;
            countText.text = $"{count}/{max}";
            bool textActive = max > 1; // single-stack items have nothing to show
            countText.gameObject.SetActive(textActive);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
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
            InventoryManager.OnInventoryChanged?.Invoke();
        }

        public Item GetItem() => item;

    }// DraggableItem ===
}
