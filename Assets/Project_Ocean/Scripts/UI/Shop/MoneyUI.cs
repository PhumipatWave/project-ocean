using UnityEngine;
using TMPro;


namespace Game.Testing
{
    public class MoneyUI : MonoBehaviour
    {
        public PlayerMoney playerMoney;
            public TMP_Text moneyText;
        
            void Update()
            {
                moneyText.text = "Money: " + playerMoney.money;
            }
    }
}
