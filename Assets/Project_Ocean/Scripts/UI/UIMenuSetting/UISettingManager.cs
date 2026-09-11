using UnityEngine;

namespace Game.Testing
{
    public class UISettingManager : MonoBehaviour
    {
        public GameObject menuPanel;
        public GameObject inventoryPanel;
        public GameObject questPanel;
        //public GameObject savePanel;
        public GameObject settingPanel;

        public void OpenInventory()
        {
            CloseAll();
            inventoryPanel.SetActive(true);
        }

        public void OpenQuest()
        {
            CloseAll();
            questPanel.SetActive(true);
        }

        /*public void OpenSave()
        {
            CloseAll();
            savePanel.SetActive(true);
        }*/

        public void OpenSetting()
        {
            CloseAll();
            settingPanel.SetActive(true);
        }

        public void CloseAll()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (questPanel != null) questPanel.SetActive(false);
            //if (savePanel != null) savePanel.SetActive(false);
            if (settingPanel != null) settingPanel.SetActive(false);
        }

        public void BackToMenu()
        {
            CloseAll();
            menuPanel.SetActive(true);
        }
    }
}
