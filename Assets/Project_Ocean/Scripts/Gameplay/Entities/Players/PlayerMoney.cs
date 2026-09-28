using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class PlayerMoney : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private int money = 100;

        /// <summary>New balance after any change — QuestManager re-checks RequiredMoney steps on it.</summary>
        public static event System.Action<int> OnMoneyChanged;

        /// <summary>Signed amount of a player-visible change (gain or spend); never raised by save load.</summary>
        public static event System.Action<int> OnMoneyDelta;

        public int Money => money;
        public int CurrentMoney => money;

        private void OnEnable() => QuestManager.OnMoneyRewarded += AddMoney;
        private void OnDisable() => QuestManager.OnMoneyRewarded -= AddMoney;

        public void AddMoney(int amount)
        {
            money += amount;
            if (amount != 0) OnMoneyDelta?.Invoke(amount);
            OnMoneyChanged?.Invoke(money);
        }

        /// <summary>Sets the balance with no notification — save load only.</summary>
        public void SetMoneySilent(int amount)
        {
            money = amount;
            OnMoneyChanged?.Invoke(money);
        }

        public bool Use(int amount)
        {
            if (money < amount)
                return false;

            money -= amount;
            if (amount != 0) OnMoneyDelta?.Invoke(-amount);
            OnMoneyChanged?.Invoke(money);
            return true;
        }

        public void Collect(SaveData data) => data.money = CurrentMoney;

        public void Apply(SaveData data) => SetMoneySilent(data.money);
    }
}
