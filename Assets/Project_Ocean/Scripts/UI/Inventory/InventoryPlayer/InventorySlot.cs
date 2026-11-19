using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler, ISubmitHandler
    {
        public static event Action<InventorySlot> OnShopSlotClicked;

        // Gamepad has no drag: first Submit picks a slot, a second Submit on another slot swaps them.
        private static InventorySlot picked;

        /// <summary>Slot picked with gamepad Submit and waiting for a target, or null.</summary>
        public static InventorySlot Picked => picked;

        public Image image;
        public Color SelectedColor, notSelectedColor;
        [SerializeField] private Color pickedColor = Color.cyan;
        public bool isSelected = false;

        public bool isUnlocked = true;
        public bool isShopSlot = false;

        private void Awake()
        {
            image = GetComponent<Image>();
            Deselect();
            gameObject.SetActive(isUnlocked);
        }

        private void OnEnable() => InventoryManager.OnInventoryClosed += HandleInventoryClosed;

        private void OnDisable()
        {
            InventoryManager.OnInventoryClosed -= HandleInventoryClosed;
            HandleInventoryClosed();
        }

        private void HandleInventoryClosed()
        {
            if (picked == this) ClearPick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!isUnlocked) return;

            if (isShopSlot)
            {
                OnShopSlotClicked?.Invoke(this);
                return;
            }

            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            if (picked == null)
            {
                if (inventory.GetStack(inventory.IndexOfSlot(transform)).IsEmpty) return;
                picked = this;
                image.color = pickedColor;
                return;
            }

            if (picked != this)
                inventory.SwapSlots(picked.transform, transform);
            ClearPick();
        }

        public static void ClearPick()
        {
            InventorySlot slot = picked;
            picked = null;
            if (slot == null) return;
            if (slot.isSelected) slot.Select();
            else slot.Deselect();
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
