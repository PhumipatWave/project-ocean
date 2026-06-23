using UnityEngine;

namespace Game.Testing
{
    public class PlayerMoney : MonoBehaviour
    {
        [SerializeField] private int money = 100;
        public int Money { get { return money; } }

        private void OnEnable()
        {
            QuestManager.OnMoneyRewarded += AddMoney;
        }

        private void OnDisable()
        {
            QuestManager.OnMoneyRewarded -= AddMoney;
        }

        public void AddMoney(int amount)
        {
            money += amount;
            Debug.Log("Money: " + money);
        }

        public bool Use(int amount)
        {
            if (money >= amount)
            {
                money -= amount;
                return true;
            }
            return false;
        }
        
        public int CurrentMoney
        {
            get { return money; } // หรือชื่อตัวแปรจริงของนาย
        }
        
        
        
    }
}
