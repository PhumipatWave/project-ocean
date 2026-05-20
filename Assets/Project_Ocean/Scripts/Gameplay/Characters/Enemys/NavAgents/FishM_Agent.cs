using UnityEngine;

namespace Game.Testing
{
    public class FishM_Agent : EnemyNavAgentBase
    {
        #region FieldProperties
        [SerializeField] private FishMBrainSettings brainSettings;

        private FishMBrain fishMBrain;
        #endregion

        #region Initialization
        private FishMBrainSettings BrainSettings
        {
            get
            {
                if (brainSettings == null)
                    Debug.LogWarning($"FishM_Agent on {gameObject.name} is missing brain settings!");

                return brainSettings;
            }
        }

        protected override IEnemyBrain CreateBrain()
        {
            fishMBrain = new FishMBrain(BrainSettings);
            return fishMBrain;
        }
        #endregion

        protected override void HandleBulletHit(GameObject bullet)
        {
            base.HandleBulletHit(bullet);
            (fishMBrain as IBrainHitReaction)?.OnBulletHit();
        }

        protected override void HandlePlayerCollision()
        {
            (fishMBrain as IBrainHitReaction)?.OnPlayerCollision();
        }

        #region UnityCallbacks
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (brainSettings == null)
                return;

            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawWireSphere(transform.position, brainSettings.viewDistance);

            Vector3 forward = transform.right;
            int steps = 20;
            float stepAngle = brainSettings.viewAngle / steps;
            float halfView = brainSettings.viewAngle / 2f;

            Vector3 prevPoint = transform.position + Quaternion.Euler(0, 0, -halfView) * forward * brainSettings.viewDistance;
            for (int i = 1; i <= steps; i++)
            {
                float angle = -halfView + stepAngle * i;
                Vector3 nextPoint = transform.position + Quaternion.Euler(0, 0, angle) * forward * brainSettings.viewDistance;
                Gizmos.DrawLine(prevPoint, nextPoint);
                Gizmos.DrawLine(transform.position, nextPoint);
                prevPoint = nextPoint;
            }
        }
#endif
        #endregion
    }
}