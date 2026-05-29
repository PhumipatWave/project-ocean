using UnityEngine;

namespace Game.Testing
{
    public enum EnemyFollowBehavior
    {
        Chase,
        Flee
    }

    [CreateAssetMenu(fileName = "EnemyBrainProfile", menuName = "Enemy/Enemy Brain Profile")]
    public class EnemyBrainProfile : EnemyBrainSettingsBase
    {
        [Header("Behavior")]
        public EnemyFollowBehavior followBehavior = EnemyFollowBehavior.Chase;
        public bool searchAfterLost = true;
        public bool prepareBeforeAttack;
        public bool canAttack;
        public bool alertNearbyAllies;

        [Header("Pathfinding")]
        public float fishClearanceRadius = 0.5f;
        public int maxWanderSampleTries = 20;

        [Header("Search")]
        public float searchDuration = 4f;
        public float searchRadius = 2.5f;

        [Header("Prepare")]
        public float prepareTime = 1f;

        [Header("Attack")]
        public float boostSpeed = 7f;
        public float attackRange = 1.5f;
        public float attackCooldown = 2f;
        public float attackDamage = 10f;

        [Header("Group Alert")]
        public float groupAlertRadius = 3f;

        [Header("Near-Wall Sampling")]
        public float nearPlayerSearchRadius = 5f;
        public int nearPlayerSearchSamples = 24;
        public float nearPlayerSearchStep = 0.5f;

        public float detectDistance => viewDistance;
    }
}
