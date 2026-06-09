using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance;
        [SerializeField] private GameObject upgradeUI;
        [SerializeField] private GameObject buyUI;

        [SerializeField]
        private List<UpgradeState> upgrades = new();

        private void Awake()
        {
            Instance = this;
        }

        public int GetLevel(UpgradeData data)
        {
            UpgradeState state = upgrades.Find(x => x.data == data);

            return state != null ? state.currentLevel : 0;
        }

        public void BuyUpgrade(UpgradeData data)
        {
            UpgradeState state =
                upgrades.Find(x => x.data == data);

            if (state == null)
                return;

            if (state.currentLevel >= data.maxLevel)
                return;

            int price =
                data.basePrice * (state.currentLevel + 1);

            PlayerMoney money =
                GameManager.CurPlayer.GetComponent<PlayerMoney>();

            if (!money.Use(price))
                return;

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
            UpgradeState state =
                upgrades.Find(x => x.data == data);

            if (state != null)
            {
                state.currentLevel = level;
            }
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