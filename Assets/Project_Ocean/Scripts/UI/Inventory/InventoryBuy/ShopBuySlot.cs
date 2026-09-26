using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ShopBuySlot : MonoBehaviour, IPointerClickHandler
    {
        public Image image;
        public Color selectedColor    = Color.yellow;
        public Color notSelectedColor = Color.white;

        public DraggableItem currentItem;
        public bool isSelected;

        public void SetItem(DraggableItem item)
        {
            currentItem = item;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentItem == null || currentItem.GetItem() == null)
            {
                Debug.Log("Empty slot clicked");
                return;
            }

            var shop = ShopManager.Instance;
            if (shop == null) return;

            shop.DeselectAllBuySlots();
            Select();
            shop.ShowItemDetail(currentItem.GetItem());
        }

        public void Select()
        {
            if (image != null) image.color = selectedColor;
            isSelected = true;
        }

        public void Deselect()
        {
            if (image != null) image.color = notSelectedColor;
            isSelected = false;
        }
    }
}
