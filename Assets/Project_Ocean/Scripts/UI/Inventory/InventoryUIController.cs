using UnityEngine;

namespace Game.Testing
{
    public class InventoryUIController : MonoBehaviour
    {
        private GameObject inventoryMenu;
        private AudioClip openInventorySound;

        private bool isInventoryOpen;

        public bool IsOpen => isInventoryOpen;

        public void Initialize(
            GameObject menu,
            AudioClip openSound)
        {
            inventoryMenu = menu;
            openInventorySound = openSound;

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
            if (GameManager.CurPlayer != null &&
                GameManager.CurPlayer.Audio != null &&
                openInventorySound != null)
            {
                GameManager.CurPlayer.Audio.PlayOneShot(
                    openInventorySound
                );
            }

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