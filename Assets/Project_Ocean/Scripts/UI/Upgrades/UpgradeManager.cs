using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>
    /// Land-scene upgrade panel controller. All upgrade state lives in UpgradeService —
    /// this class only handles panel navigation and forwards UI calls.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private ShopManager shopManager;

        // ──────────────────────────────────────────
        // Panel navigation
        // ──────────────────────────────────────────

        /// <summary>Opens the upgrade panel and ensures the shop is open alongside it.</summary>
        public void OpenUpgradeShop()
        {
            shopManager?.OpenShopWindow();
        }

        /// <summary>Alias kept for existing scene button bindings.</summary>
        public void OpenFromShop() => OpenUpgradeShop();

        // ──────────────────────────────────────────
        // Upgrade API — delegates to UpgradeService
        // ──────────────────────────────────────────

        public int GetLevel(UpgradeData data)
        {
            return data != null
                ? UpgradeService.Instance.GetLevel(data.type)
                : 0;
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

        public int GetPrice(UpgradeData data)
        {
            return data != null
                ? UpgradeService.Instance.GetPrice(data)
                : 0;
        }
    }
}
