using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class UISettingManager : MonoBehaviour
    {
        public GameObject menuPanel;
        public GameObject inventoryPanel;
        public GameObject questPanel;
        public GameObject settingPanel;

        public bool IsOpen
        {
            get
            {
                if (UIManager.Instance != null)
                    return UIManager.Instance.IsUIOpen;

                return
                    (menuPanel != null && menuPanel.activeSelf) ||
                    (inventoryPanel != null && inventoryPanel.activeSelf) ||
                    (questPanel != null && questPanel.activeSelf) ||
                    (settingPanel != null && settingPanel.activeSelf);
            }
        }

        public void ToggleMenu()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ToggleMainMenu();
                return;
            }

            if (IsOpen)
                CloseAll();
            else
                BackToMenu();
        }

        public void OpenInventory()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenInventory();
                return;
            }

            CloseAll();

            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);
        }

        public void OpenQuest()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenQuest();
                return;
            }

            CloseAll();

            if (questPanel != null)
                questPanel.SetActive(true);
        }

        public void OpenSetting()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenSetting();
                return;
            }

            CloseAll();

            if (settingPanel != null)
                settingPanel.SetActive(true);
        }

        public void CloseAll()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAll();
                return;
            }

            if (menuPanel != null)
                menuPanel.SetActive(false);

            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            if (questPanel != null)
                questPanel.SetActive(false);

            if (settingPanel != null)
                settingPanel.SetActive(false);
        }

        public void BackToMenu()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenMainMenu();
                return;
            }

            CloseAll();

            if (menuPanel != null)
                menuPanel.SetActive(true);
        }
    }
}