using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        public static event Action<InventorySlot> OnShopSlotClicked;

        public Image image;
        public Color SelectedColor, notSelectedColor;
        public bool isSelected = false;

        public bool isUnlocked = true;
        public bool isShopSlot = false;

        private void Awake()
        {
            image = GetComponent<Image>();
            Deselect();
            gameObject.SetActive(isUnlocked);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            if (isShopSlot)
            {
                OnShopSlotClicked?.Invoke(this);
                return;
            }

            if (InventoryManager.Instance != null)
            {
                for (int i = 0; i < InventoryManager.Instance.InventorySlots.Length; i++)
                {
                    if (InventoryManager.Instance.InventorySlots[i] == this)
                    {
                        InventoryManager.Instance.SelectSlot(i);
                        break;
                    }
                }
            }
        }

        public void Select()
        {
            image.color = Color.yellow;
            transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            isSelected = true;
        }

        public void Deselect()
        {
            image.color = Color.white;
            transform.localScale = Vector3.one;
            isSelected = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            GameObject dropped = eventData.pointerDrag;
            if (dropped == null) return;

            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            if (draggableItem == null) return;

            if (transform.childCount == 0)
                draggableItem.parentAfterDrag = transform;
        }

        public void SetLocked(bool locked)
        {
            isUnlocked = !locked;
            gameObject.SetActive(!locked);
        }
    }
}
