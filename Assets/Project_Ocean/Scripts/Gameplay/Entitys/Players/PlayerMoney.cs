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

            if (amount > 0)
                NotificationManager.Notify($"+{amount} Money");
        }

        /// <summary>
        /// Adds money without a UI notification — used when restoring saved state,
        /// so loading a save doesn't show a spurious "+money" toast.
        /// </summary>
        public void AddMoneySilent(int amount)
        {
            money += amount;
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
