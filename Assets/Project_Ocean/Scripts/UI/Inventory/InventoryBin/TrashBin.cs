using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class TrashBin : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IDropHandler
    {
        [Header("Trash Bin Visual")]
        [SerializeField] private Image trashImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite hoverSprite;

        [Header("Hover Effect")]
        [SerializeField] private float hoverScale = 1.1f;

        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;

            if (trashImage == null)
                trashImage = GetComponent<Image>();

            ResetVisual();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DraggableItem draggableItem =
                eventData.pointerDrag?.GetComponent<DraggableItem>();

            if (draggableItem == null)
                return;

            ShowHoverVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetVisual();
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropped = eventData.pointerDrag;

            if (dropped == null)
                return;

            DraggableItem draggableItem =
                dropped.GetComponent<DraggableItem>();

            if (draggableItem == null)
                return;

            ResetVisual();

            InventoryManager.Instance?.DiscardDraggedItem(draggableItem);
        }

        private void ShowHoverVisual()
        {
            if (trashImage != null && hoverSprite != null)
                trashImage.sprite = hoverSprite;

            transform.localScale = originalScale * hoverScale;
        }

        private void ResetVisual()
        {
            if (trashImage != null && normalSprite != null)
                trashImage.sprite = normalSprite;

            transform.localScale = originalScale;
        }
    }
}