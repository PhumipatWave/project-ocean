using UnityEngine;
using TMPro;


namespace Game.Testing
{
    public class MoneyUI : MonoBehaviour
    {
        public TMP_Text moneyText;
        
        void Update()
        {
            moneyText.text = "Money: " + GameManager.CurPlayer.GetComponent<PlayerMoney>().Money;
        }
    }
}
