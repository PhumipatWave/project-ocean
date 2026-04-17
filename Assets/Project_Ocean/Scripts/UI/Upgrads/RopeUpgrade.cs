using UnityEngine;
using TMPro;

namespace Game.Testing
{
    public class RopeUpgrade : MonoBehaviour
    {
        public PlayerMoney playerMoney;
        public PlayerOxygenTube rope;

        [Header("Upgrade")]
        public int level = 1;
        public int basePrice = 100;

        [Header("Rope")]
        public float lengthIncrease = 20f;

        [Header("UI")]
        public TMP_Text priceText;
        public TMP_Text lengthText;

        void Update()
        {
            priceText.text = "$" + GetPrice();
            lengthText.text = "Max Length: " + ropeMax();
        }

        public void UpgradeRope()
        {
            int price = GetPrice();

            if (!playerMoney.Use(price))
            {
                Debug.Log("No Money to Upgrade!!");
                return;
            }

            rope.IncreaseMaxLength(lengthIncrease);
            level++;

            Debug.Log("Upgraded Rope!!");
        }

        int GetPrice()
        {
            return basePrice * level;
        }

        float ropeMax()
        {
            return rope.GetMaxLength();
        }
    }
}
