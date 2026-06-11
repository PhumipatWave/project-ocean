using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        public Image image;
        public Color SelectedColor, notSelectedColor;
        public bool isSelected = false;

        // Lock Slot
        public bool isUnlocked = true;
        public bool isShopSlot = false;

        /// <summary>
        /// Assign this in the Inspector only on slots that live inside the Shop panel.
        /// Leave empty on regular player inventory slots.
        /// </summary>
        [SerializeField] private ShopManager shopManager;

        private void Awake()
        {
            image = GetComponent<Image>();
            Deselect();
            gameObject.SetActive(isUnlocked);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("CLICK SLOT : " + gameObject.name);
            Debug.Log("isShopSlot = " + isShopSlot);

            if (!isUnlocked)
            {
                Debug.Log("SLOT LOCKED");
                return;
            }

            // =========================
            // SHOP SELL SLOT
            // =========================
            if (isShopSlot)
            {
                if (shopManager != null)
                {
                    Debug.Log("SHOP MODE");
                    shopManager.DeselectAllShopSlots();
                    Select();
                    shopManager.UpdateClickedItemPriceDisplay();
                }
                else
                {
                    Debug.LogWarning($"[InventorySlot] isShopSlot=true but no ShopManager found for '{gameObject.name}'");
                }
                return;
            }

            // =========================
            // PLAYER INVENTORY SLOT
            // =========================
            Debug.Log("PLAYER MODE");

            if (InventoryManager.Instance != null)
            {
                for (int i = 0; i < InventoryManager.Instance.InventorySlots.Length; i++)
                {
                    if (InventoryManager.Instance.InventorySlots[i] == this)
                    {
                        InventoryManager.Instance.SendMessage(
                            "ChangeSelectedSlot",
                            i,
                            SendMessageOptions.DontRequireReceiver);
                        break;
                    }
                }
            }
        }

        public void Select()
        {
            Debug.Log("SELECT() CALLED");
            image.color = Color.yellow;
            transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            isSelected = true;
        }

        public void Deselect()
        {
            Debug.Log("DESELECT() CALLED : " + gameObject.name);
            image.color = Color.white;
            transform.localScale = Vector3.one;
            isSelected = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!isUnlocked) return;

            GameObject dropped = eventData.pointerDrag;
            if (dropped == null) return;

            Draggableltem draggableItem = dropped.GetComponent<Draggableltem>();
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
