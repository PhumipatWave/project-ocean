using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class InventoryUIController : MonoBehaviour
    {
        private GameObject inventoryMenu;

        private bool isInventoryOpen;

        public bool IsOpen => isInventoryOpen;

        public void Initialize(GameObject menu)
        {
            inventoryMenu = menu;

            isInventoryOpen =
                inventoryMenu != null &&
                inventoryMenu.activeSelf;
        }

        public void ToggleInventory()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Toggle(
                    UIManager.UIType.Inventory
                );

                isInventoryOpen =
                    UIManager.Instance.CurrentUI ==
                    UIManager.UIType.Inventory;

                return;
            }

            // Fallback
            if (isInventoryOpen)
                HideInventory();
            else
                ShowInventory();
        }

        public void ShowInventory()
        {
            // UI sound: Master channel (not a gameplay effect, so only the master slider scales it).
            AudioManager.Instance?.PlayMaster(SoundKeys.OpenInventory);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenInventory();
                isInventoryOpen = true;
                return;
            }

            // Fallback
            if (inventoryMenu != null)
                inventoryMenu.SetActive(true);

            isInventoryOpen = true;
        }

        public void HideInventory()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Close(
                    UIManager.UIType.Inventory
                );

                isInventoryOpen = false;
                return;
            }

            if (inventoryMenu != null)
                inventoryMenu.SetActive(false);

            isInventoryOpen = false;
        }
    }
}