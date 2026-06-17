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

        public Draggableltem currentItem;
        public bool isSelected;

        [SerializeField] private ShopManager shopManager;

        public void SetItem(Draggableltem item)
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

            if (shopManager == null) return;

            shopManager.DeselectAllBuySlots();
            Select();
            shopManager.ShowItemDetail(currentItem.GetItem());
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
