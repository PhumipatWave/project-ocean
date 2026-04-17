using UnityEngine;

namespace Game.Testing
{
    public class UIManager : MonoBehaviour
    {
        public GameObject ShopPanel;
        public GameObject upgradePanel;

        void Start()
        {
            ShowShopMain();
        }

        public void ShowShopMain()
        {
            ShopPanel.SetActive(true);
            upgradePanel.SetActive(false);
        }

        public void ShowUpgrade()
        {
            ShopPanel.SetActive(false);
            upgradePanel.SetActive(true);
        }
    } // UIManager ===
}
