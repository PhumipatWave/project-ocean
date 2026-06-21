using UnityEngine;

namespace Game.Testing
{
    public class InventoryUIController : MonoBehaviour
    {
        private GameObject inventoryMenu;
        private AudioClip openInventorySound;
        private bool isInventoryOpen;
        public bool IsOpen => isInventoryOpen;

        public void Initialize(GameObject menu, AudioClip openSound)
        {
            inventoryMenu = menu;
            openInventorySound = openSound;
            isInventoryOpen = inventoryMenu != null && inventoryMenu.activeSelf;
        }

        public void ToggleInventory()
        {
            if (isInventoryOpen)
                HideInventory();
            else
                ShowInventory();
        }

        public void ShowInventory()
        {
            if (GameManager.CurPlayer != null && GameManager.CurPlayer.Audio != null && openInventorySound != null)
                GameManager.CurPlayer.Audio.PlayOneShot(openInventorySound);

            Time.timeScale = 0;
            if (inventoryMenu != null)
                inventoryMenu.SetActive(true);

            isInventoryOpen = true;
        }

        public void HideInventory()
        {
            Time.timeScale = 1;
            if (inventoryMenu != null)
                inventoryMenu.SetActive(false);

            isInventoryOpen = false;
        }
    }
}
