namespace Game.Core
{
    public static class EnemyEvents
    {
        /// <summary>Raised once when an enemy dies. Payload = the enemy's id (Enemy.EnemyId).</summary>
        public static event System.Action<string> OnEnemyKilled;

        public static void RaiseEnemyKilled(string enemyId)
        {
            OnEnemyKilled?.Invoke(enemyId);
        }
    }
}
