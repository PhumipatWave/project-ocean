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

            if (isShopSlot)
            {
                var shop = ShopManager.Instance;
                if (shop != null)
                {
                    Debug.Log("SHOP MODE");
                    shop.DeselectAllShopSlots();
                    Select();
                    shop.UpdateClickedItemPriceDisplay();
                }
                else
                {
                    Debug.LogWarning($"[InventorySlot] isShopSlot=true but ShopManager is not in the current scene ('{gameObject.name}')");
                }
                return;
            }

            Debug.Log("PLAYER MODE");

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
