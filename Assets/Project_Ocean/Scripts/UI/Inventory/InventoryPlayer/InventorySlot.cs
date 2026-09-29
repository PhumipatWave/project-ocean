using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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

            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            int index = inventory.IndexOfSlot(transform);
            if (index >= 0) inventory.SelectSlot(index);
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
            // Shop slots only mirror the inventory; accepting a drop would strand the view there.
            if (!isUnlocked || isShopSlot) return;

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
