using UnityEngine;
using TMPro;

namespace Game.Testing
{
    public class MoneyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;

        private PlayerMoney playerMoney;

        private void Start()
        {
            if (GameManager.CurPlayer != null)
                playerMoney = GameManager.CurPlayer.GetComponent<PlayerMoney>();
        }

        private void Update()
        {
            if (playerMoney != null && moneyText != null)
                moneyText.text = "Money: " + playerMoney.Money;
        }
    }
}
