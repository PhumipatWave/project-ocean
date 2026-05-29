using UnityEngine;

namespace Game.Testing
{
    public class FishS_Agent : EnemyNavAgentBase
    {
        #region FieldProperties
        [SerializeField] private FishSBrainSettings brainSettings;

        [Header("Group Settings")]
        public FishS_Agent leader;
        public int groupID;

        [SerializeField] private float followDistance  = 1.5f;
        [SerializeField] private float followStrength  = 0.6f;
        [SerializeField] private float separationRadius   = 0.8f;
        [SerializeField] private float separationStrength = 0.4f;

        private FishSBrain fishSBrain;

        public bool IsLeader => leader == null;
        #endregion

        #region Initialization
        private FishSBrainSettings BrainSettings
        {
            get
            {
                if (brainSettings == null)
                    brainSettings = ScriptableObject.CreateInstance<FishSBrainSettings>();

                return brainSettings;
            }
        }

        protected override IEnemyBrain CreateBrain()
        {
            fishSBrain = new FishSBrain(BrainSettings);
            return fishSBrain;
        }
        #endregion

        #region GroupBehavior
        /// <summary>
        /// ตั้ง leader ให้ follower — เรียกจาก Spawner หรือ GroupManager
        /// </summary>
        public void SetLeader(FishS_Agent newLeader, int newGroupID)
        {
            leader  = newLeader;
            groupID = newGroupID;
        }

        private void ApplyFollowSteering()
        {
            if (leader == null || leader.isEnemyDead)
                return;

            Vector2 toLeader = (Vector2)leader.transform.position - (Vector2)transform.position;
            float dist = toLeader.magnitude;

            Vector2 steer = Vector2.zero;

            if (dist > followDistance)
                steer += toLeader.normalized * followStrength;

            steer += GetSeparationForce();

            // Note: group steering is visual only — brain FSM drives actual movement
        }

        private Vector2 GetSeparationForce()
        {
            Vector2 force = Vector2.zero;
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
            foreach (var col in nearby)
            {
                if (col.gameObject == gameObject) continue;
                if (!col.TryGetComponent<FishS_Agent>(out var other)) continue;
                if (other.groupID != groupID) continue;

                Vector2 away = (Vector2)transform.position - (Vector2)other.transform.position;
                float dist = away.magnitude;
                if (dist > 0.001f)
                    force += away.normalized * (separationRadius - dist) * separationStrength;
            }
            return force;
        }
        #endregion

        protected override void HandleBulletHit(GameObject bullet)
        {
            if (isEnemyDead) return;
            if (bullet == null || !bullet.CompareTag(playerBulletTag)) return;

            (fishSBrain as IBrainHitReaction)?.OnBulletHit();
        }

        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;
            (fishSBrain as IBrainHitReaction)?.OnPlayerCollision();
        }

        #region UnityCallbacks
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (brainSettings == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, brainSettings.fishClearanceRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * brainSettings.wallCheckDistance);

            Gizmos.color = Color.green;
            Vector2 facingDir = Application.isPlaying && facingCtr != null
                ? facingCtr.GetFacingDirection() : transform.right;

            Vector3 leftDir  = Quaternion.Euler(0f, 0f,  brainSettings.viewAngle * 0.5f) * facingDir;
            Vector3 rightDir = Quaternion.Euler(0f, 0f, -brainSettings.viewAngle * 0.5f) * facingDir;

            Gizmos.DrawLine(transform.position, transform.position + leftDir  * brainSettings.detectDistance);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * brainSettings.detectDistance);
            Gizmos.DrawWireSphere(transform.position, brainSettings.detectDistance);
        }
#endif
        #endregion
    }
}