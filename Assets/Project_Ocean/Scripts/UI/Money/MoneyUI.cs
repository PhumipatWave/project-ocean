using UnityEngine;
using TMPro;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class MoneyUI : MonoBehaviour
    {
        [Header("Money Display")]
        [SerializeField] private TMP_Text moneyText;

        [Tooltip("On: \"Money: 1,250\". Off: \"1,250\".")]
        [SerializeField] private bool showMoneyLabel = false;

        private PlayerMoney playerMoney;

        private void Start()
        {
            FindPlayerMoney();
            Refresh();
        }

        private void Update()
        {
            if (playerMoney == null)
                FindPlayerMoney();

            Refresh();
        }

        private void FindPlayerMoney()
        {
            if (PlayerRegistry.Local != null)
            {
                playerMoney =
                    PlayerRegistry.Local.GetComponent<PlayerMoney>();
            }
        }

        private void Refresh()
        {
            if (playerMoney == null || moneyText == null)
                return;

            string value = playerMoney.Money.ToString("N0");

            if (showMoneyLabel)
            {
                moneyText.text = "Money: " + value;
            }
            else
            {
                moneyText.text = value;
            }
        }
    }
}