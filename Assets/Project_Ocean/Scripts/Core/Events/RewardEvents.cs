namespace Game.Core
{
    /// <summary>Money granted by gameplay (quest rewards); PlayerMoney adds it to the balance.</summary>
    public static class RewardEvents
    {
        public static event System.Action<int> OnMoneyRewarded;

        public static void RaiseMoneyRewarded(int amount) => OnMoneyRewarded?.Invoke(amount);
    }
}
