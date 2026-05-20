using UnityEngine;

namespace Game.Testing
{
    public class FishL_Agent : EnemyNavAgentBase
    {
        #region FieldProperties
        [SerializeField] private FishLBrainSettings brainSettings;

        private FishLBrain fishLBrain;
        #endregion

        #region Initialization
        private FishLBrainSettings BrainSettings
        {
            get
            {
                if (brainSettings == null)
                    brainSettings = ScriptableObject.CreateInstance<FishLBrainSettings>();

                return brainSettings;
            }
        }

        protected override IEnemyBrain CreateBrain()
        {
            fishLBrain = new FishLBrain(BrainSettings);
            return fishLBrain;
        }
        #endregion

        protected override void HandleBulletHit(GameObject bullet)
        {
            if (isEnemyDead) return;
            if (bullet == null || !bullet.CompareTag(playerBulletTag)) return;

            (fishLBrain as IBrainHitReaction)?.OnBulletHit();
        }

        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;
            (fishLBrain as IBrainHitReaction)?.OnPlayerCollision();
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

            Vector3 leftDir = Quaternion.Euler(0f, 0f, brainSettings.viewAngle * 0.5f) * facingDir;
            Vector3 rightDir = Quaternion.Euler(0f, 0f, -brainSettings.viewAngle * 0.5f) * facingDir;

            Gizmos.DrawLine(transform.position, transform.position + leftDir * brainSettings.detectDistance);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * brainSettings.detectDistance);
            Gizmos.DrawWireSphere(transform.position, brainSettings.detectDistance);
        }
#endif
        #endregion
    }
}