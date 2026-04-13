using UnityEngine;
using TMPro;

namespace Game.Testing
{
    public class DamageUpgrade : MonoBehaviour
    {
        public PlayerMoney playerMoney;

        [Header("Damage")]
        public float currentDamage = 10f;
        public float damageIncrease = 5f;

        [Header("Upgrade")]
        public int level = 1;
        public int basePrice = 50;

        [Header("UI")]
        public TMP_Text priceText;
        public TMP_Text damageText;

        void Update()
        {
            priceText.text = "$" + GetPrice();
            damageText.text = "Damage: " + currentDamage;
            
        } // Update ===

        public void UpgradeDamage()
        {
            int price = GetPrice();

            if (!playerMoney.Use(price))
            {
                Debug.Log("No Money to Upgrade!!");
                return;
            }

            currentDamage += damageIncrease;
            level++;

            Debug.Log("Damage Upgrade: " + currentDamage);
            
        } // UpgradeDamage ===

        int GetPrice()
        {
            return basePrice * level;
        } // GetPrice ===
        
    } // DamageUpgrade ===
}
