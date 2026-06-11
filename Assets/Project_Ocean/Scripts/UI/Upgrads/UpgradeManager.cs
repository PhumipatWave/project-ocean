using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Land-scene-local upgrade manager.
    /// No static Instance — callers in the same scene wire this via Inspector.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private GameObject upgradeUI;
        [SerializeField] private GameObject buyUI;

        [SerializeField]
        private List<UpgradeState> upgrades = new();

        // No Awake — nothing to initialise as a singleton.

        public int GetLevel(UpgradeData data)
        {
            UpgradeState state = upgrades.Find(x => x.data == data);
            return state != null ? state.currentLevel : 0;
        }

        public void BuyUpgrade(UpgradeData data)
        {
            UpgradeState state = upgrades.Find(x => x.data == data);
            if (state == null) return;
            if (state.currentLevel >= data.maxLevel) return;

            int price = data.basePrice * (state.currentLevel + 1);

            PlayerMoney money = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (!money.Use(price)) return;

            switch (data.type)
            {
                case UpgradeType.Inventory:
                    InventoryManager.Instance.UnlockNextSlot();
                    break;

                case UpgradeType.Rope:
                    GameManager.CurPlayer
                        .GetComponent<PlayerOxygenTube>()
                        .IncreaseMaxLength(data.value);
                    break;
            }

            state.currentLevel++;
        }

        public void SetLevel(UpgradeData data, int level)
        {
            UpgradeState state = upgrades.Find(x => x.data == data);
            if (state != null)
                state.currentLevel = level;
        }

        public void OpenBuyUI()
        {
            upgradeUI.SetActive(false);
            buyUI.SetActive(true);
        }

        public void BackToUpgradeUI()
        {
            buyUI.SetActive(false);
            upgradeUI.SetActive(true);
        }
    }
}
