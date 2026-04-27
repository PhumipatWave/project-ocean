using UnityEngine;
using TMPro;


namespace Game.Testing
{
    public class MoneyUI : MonoBehaviour
    {
        public TMP_Text moneyText;
        
        void Update()
        {
            if (GameManager.CurPlayer != null)
            {
                PlayerMoney pm = GameManager.CurPlayer.GetComponent<PlayerMoney>();
                if (pm != null && moneyText != null)
                {
                    // ใช้ .Money หรือ .CurrentMoney ตามที่นิยามไว้ใน PlayerMoney.cs
                    moneyText.text = "Money: " + pm.Money;
                }
            }
        }
    }
}
