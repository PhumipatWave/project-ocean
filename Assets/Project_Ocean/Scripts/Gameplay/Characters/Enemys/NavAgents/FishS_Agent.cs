using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    public class FishS_Agent : EnemyNavAgentBase
    {
        #region FieldProperties
        [SerializeField] private FishSBrainSettings brainSettings;
        [SerializeField] private Transform playerTransform;

        public FishS_Agent leader;
        public int groupID;

        private FishSBrain fishSBrain;
        private bool isBouncing;

        public Vector2 MoveDirection => fishSBrain != null ? fishSBrain.CurrentDirection : Vector2.zero;
        #endregion

        #region Initialization
        protected override bool requiresPlayer => false;

        protected override EnemyFacingController.FacingTurnMode defaultFacingTurnMode =>
            EnemyFacingController.FacingTurnMode.TurnSpeed;

        private FishSBrainSettings BrainSettings
        {
            get
            {
                if (brainSettings == null)
                    Debug.LogWarning($"FishS_Agent on {gameObject.name} is missing brain settings!");

                return brainSettings;
            }
        }

        protected override IEnemyBrain CreateBrain()
        {
            fishSBrain = new FishSBrain(BrainSettings);
            return fishSBrain;
        }

        protected override void OnBrainInitialized()
        {
            if (base.playerTransform != null)
                playerTransform = base.playerTransform;
            else if (GameManager.CurPlayer != null)
                playerTransform = GameManager.CurPlayer.transform;

            fishSBrain?.SetPlayerTransform(playerTransform);
        }

        protected override bool CanTick()
        {
            if (isEnemyDead)
            {
                OnEnemyDead();
                return false;
            }

            if (isBouncing)
                return false;

            return navAgent != null && navAgent.isOnNavMesh;
        }
        #endregion

        #region UnityCallbacks
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if (isEnemyDead || fishSBrain == null)
                return;

            if (((1 << collision.gameObject.layer) & BrainSettings.wallLayer) == 0)
                return;

            Vector2 normal = collision.contacts[0].normal;
            fishSBrain.ReflectDirection(normal, randomizePanic: true);

            StopAllCoroutines();
            StartCoroutine(WallBounceRoutine());
        }
        #endregion

        public void FleeFrom(Vector2 targetPos)
        {
            fishSBrain?.FleeFrom(targetPos);
        }

        protected override void HandleBulletHit(GameObject bullet)
        {
            if (isEnemyDead)
                return;

            if (bullet == null || !bullet.CompareTag(playerBulletTag))
                return;

            FleeFrom(bullet.transform.position);
        }

        protected override void HandlePlayerCollision()
        {
            if (playerTransform != null)
                FleeFrom(playerTransform.position);
        }

        private IEnumerator WallBounceRoutine()
        {
            isBouncing = true;

            int attempts = 0;
            bool freeFromWall = false;

            while (attempts < 10 && !freeFromWall)
            {
                if (!fishSBrain.IsDirectionBlocked())
                {
                    freeFromWall = true;
                    break;
                }

                float angle = Random.Range(45f, 135f);
                fishSBrain.NudgeDirection(angle);
                attempts++;
                yield return null;
            }

            fishSBrain.ApplyBounceDestination();
            isBouncing = false;
        }
    }
}