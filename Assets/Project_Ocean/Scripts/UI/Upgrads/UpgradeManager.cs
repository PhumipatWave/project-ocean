using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Land-scene upgrade panel UI. All upgrade state lives in UpgradeService —
    /// this class only handles panel navigation and forwards UI calls.
    /// Public API (GetLevel / BuyUpgrade / SetLevel) is unchanged so UpgradeSlot
    /// and existing button bindings keep working.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private GameObject upgradeUI;
        [SerializeField] private GameObject buyUI;

        /// <summary>
        /// Cross-reference to ShopManager — drag in Inspector.
        /// Used to navigate back to the Shop view.
        /// </summary>
        [Header("Navigation")]
        [SerializeField] private ShopManager shopManager;

        private void Awake()
        {
            // Hide on scene start — panel is inactive by default.
            gameObject.SetActive(false);
        }

        // ──────────────────────────────────────────
        // Panel navigation
        // ──────────────────────────────────────────

        /// <summary>Called by ShopManager.OpenUpgradeView() — shows this panel.</summary>
        public void OpenFromShop()
        {
            gameObject.SetActive(true);
            // Start on the upgrade list view, not the buy confirm view
            if (upgradeUI != null) upgradeUI.SetActive(true);
            if (buyUI != null)     buyUI.SetActive(false);
        }

        /// <summary>Called by the Back button — hides upgrade panel, reopens shop.</summary>
        public void BackToShop()
        {
            gameObject.SetActive(false);

            if (shopManager != null)
                shopManager.OpenShopWindow();
            else
                Debug.LogWarning("[UpgradeManager] shopManager ref not assigned in Inspector");
        }

        /// <summary>Called when upgrade is fully dismissed. Pair with LandUI.CloseUI() on the close button.</summary>
        public void CloseAll()
        {
            gameObject.SetActive(false);
        }

        public void OpenBuyUI()
        {
            if (upgradeUI != null) upgradeUI.SetActive(false);
            if (buyUI != null)     buyUI.SetActive(true);
            if (shopManager != null) shopManager.SelectFirstBuySlot();
        }

        public void BackToUpgradeUI()
        {
            if (buyUI != null)     buyUI.SetActive(false);
            if (upgradeUI != null) upgradeUI.SetActive(true);
        }

        // ──────────────────────────────────────────
        // Upgrade API — delegates to UpgradeService (single source of truth)
        // ──────────────────────────────────────────

        public int GetLevel(UpgradeData data)
        {
            return data != null ? UpgradeService.Instance.GetLevel(data.type) : 0;
        }

        public void BuyUpgrade(UpgradeData data)
        {
            UpgradeService.Instance.TryBuyUpgrade(data);
        }

        public void SetLevel(UpgradeData data, int level)
        {
            if (data != null)
                UpgradeService.Instance.SetLevel(data.type, level);
        }
    }
}
