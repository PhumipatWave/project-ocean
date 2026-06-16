using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Game.Testing
{
    public class Draggableltem : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
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
            countText.text = count.ToString();
            bool textActive = count > 1;
            countText.gameObject.SetActive(textActive);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Begin Drag");

            parentAfterDrag = transform.parent;

            transform.SetParent(transform.root, true);

            transform.SetAsLastSibling();
            image.raycastTarget = false;

        }// OnBeginDrag ===

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging");
            transform.position = Input.mousePosition;
            
        }// OnDrag === 

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("End Drag");

            transform.SetParent(parentAfterDrag);

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            image.raycastTarget = true;

            Invoke(nameof(RefreshInventoryEvent), 0.05f);
        }// OnEndDrag ===
            
        
        public Item GetItem()
        {
            return item;

        } // Item ===

        private void RefreshInventoryEvent()
        {
            InventoryManager.OnInventoryChanged?.Invoke();
        }

    }// Draggableltem ===
}
