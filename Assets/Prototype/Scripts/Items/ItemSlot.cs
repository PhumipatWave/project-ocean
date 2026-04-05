using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class ItemSlot : MonoBehaviour , IPointerClickHandler
    {
        // == Item Data ===
        public string itemName;
        public int quantity;
        public Sprite itemSprite;
        public bool isFull;
        
        // == Item Slot ===
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Image itemImage;

        public GameObject selectedShader;
        public bool thisItemSelected;
        
        private InventoryManager inventoryManager;

        private void Start()
        {
            inventoryManager = GameObject.Find("Inventory Canvas").GetComponent<InventoryManager>();
        }

        public void AddItem(string itemName, int quantity, Sprite itemSprite)
        {
            this.itemName = itemName;
            this.quantity = quantity;
            this.itemSprite = itemSprite;
            isFull = true;
            
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
            itemImage.sprite = itemSprite;
            
        } // AddItem


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick();
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();
                
            }
            
        } // OnPointerClick ===

        public void OnLeftClick()
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            
        } // OnLeftClick ===

        public void OnRightClick()
        {
            
        } // OnRightClick ===
    }
}
