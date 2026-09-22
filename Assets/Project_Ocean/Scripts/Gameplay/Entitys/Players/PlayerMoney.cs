using UnityEngine;

namespace Game.Testing
{
    public class PlayerMoney : MonoBehaviour
    {
        [SerializeField] private int money = 100;

        /// <summary>Fires after every balance change (AddMoney/AddMoneySilent/Use) — QuestManager
        /// listens to re-check Delivery steps gated on RequiredMoney.</summary>
        public static event System.Action<int> OnMoneyChanged;

        public int Money
        {
            get { return money; }
        }

        private void OnEnable()
        {
            // Quest Reward เพิ่มเงินแบบไม่เด้ง Notification
            QuestManager.OnMoneyRewarded += AddMoneySilent;
        }

        private void OnDisable()
        {
            QuestManager.OnMoneyRewarded -= AddMoneySilent;
        }

        // =========================================================
        // ADD MONEY
        // =========================================================

        public void AddMoney(int amount)
        {
            money += amount;

            Debug.Log("Money: " + money);

            // การเพิ่มเงินทั่วไปยังสามารถแจ้งเตือนได้
            if (amount > 0)
            {
                NotificationManager.Notify(
                    $"+{amount} Money"
                );
            }

            OnMoneyChanged?.Invoke(money);
        }

        // =========================================================
        // ADD MONEY SILENT
        // =========================================================

        /// <summary>
        /// เพิ่มเงินโดยไม่แสดง Notification
        /// ใช้สำหรับ Quest Reward
        /// และการโหลด Save
        /// </summary>
        public void AddMoneySilent(int amount)
        {
            money += amount;

            Debug.Log(
                "Money (Silent): " + money
            );

            OnMoneyChanged?.Invoke(money);
        }

        // =========================================================
        // USE MONEY
        // =========================================================

        public bool Use(int amount)
        {
            if (money >= amount)
            {
                money -= amount;
                OnMoneyChanged?.Invoke(money);
                return true;
            }

            return false;
        }

        // =========================================================
        // CURRENT MONEY
        // =========================================================

        public int CurrentMoney
        {
            get { return money; }
        }
    }
}