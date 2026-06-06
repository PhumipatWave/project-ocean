using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ShopBuySlot : MonoBehaviour, IPointerClickHandler
    {
        public Image image;
        public Color selectedColor = Color.yellow;
        public Color notSelectedColor = Color.white;

        private void Awake()
        {
            // ถ้าลืมลากใส่ใน Inspector ให้มันหาเองที่ตัวมันเอง
            if (image == null) image = GetComponent<Image>();
            Deselect();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Clicked on Shop Slot: " + gameObject.name);

            ShopBuySlot[] allShopSlots =
                transform.parent.GetComponentsInChildren<ShopBuySlot>();

            foreach (var slot in allShopSlots)
            {
                slot.Deselect();
            }

            Select();

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.UpdateClickedItemPriceDisplay();
            }
        }

        public void Select()
        {
            if (image != null)
            {
                image.color = selectedColor;
                Debug.Log(gameObject.name + " is Selected");
            }
        }

        public void Deselect()
        {
            if (image != null) image.color = notSelectedColor;
        }
    }
}