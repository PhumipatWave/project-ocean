using UnityEngine;

namespace Game.Testing
{
    public class FishSFollowState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Follow;

        private readonly FishSBrainSettings settings;
        private readonly FishSVisionHelper  vision;

        private Vector2 currentDirection;
        private float   hitChaseTimer;
        private float   followTimer;
        private bool    hasSeenPlayer;

        public Vector3 LastKnownPlayerPosition { get; set; }

        public void SetHitChaseTimer(float duration) { hitChaseTimer = duration; }

        public FishSFollowState(FishSBrainSettings settings, FishSVisionHelper vision)
        {
            this.settings = settings;
            this.vision   = vision;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            SetAgentSpeed(settings.speed);
            followTimer   = settings.followAfterLost;
            hasSeenPlayer = false;

            if (enemyBrainContext.PlayerTransform != null)
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;

            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.ShowAttackWarning();
        }

        public override void Tick(float deltaTime)
        {
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;
            if (followTimer   > 0f) followTimer   -= deltaTime;

            if (enemyBrainContext.PlayerTransform == null)
            {
                TransitionOnLost();
                return;
            }

            bool canSee  = vision.CheckLineOfSight();
            bool alerted = hitChaseTimer > 0f;

            if (canSee || alerted)
            {
                hasSeenPlayer           = true;
                LastKnownPlayerPosition = enemyBrainContext.PlayerTransform.position;
                followTimer             = settings.followAfterLost;
                enemyBrainContext.AlertUI?.SetSeeingPlayer(canSee);
            }

            if (!canSee && !alerted && followTimer <= 0f)
            {
                TransitionOnLost();
                return;
            }

            // คำนวณทิศหนี
            Vector3 awayDir = (enemyBrainContext.Transform.position - LastKnownPlayerPosition).normalized;
            if (awayDir == Vector3.zero)
                awayDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;

            // ถ้าใกล้ถึง dest แล้ว คำนวณใหม่เลย ไม่ให้หยุด
            float distToDest = Vector2.Distance(
                enemyBrainContext.Transform.position,
                enemyBrainContext.Agent.destination
            );

            if (distToDest < 0.5f)
            {
                bool found = false;
                foreach (float d in new float[] { 3f, 2f, 1f })
                {
                    Vector3 candidate = enemyBrainContext.Transform.position + awayDir * d;
                    candidate.z = enemyBrainContext.Transform.position.z;
                    if (enemyBrainContext.Agent.SetDestination(candidate))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Vector3 randomDir  = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
                    Vector3 randomDest = enemyBrainContext.Transform.position + randomDir * 2f;
                    randomDest.z = enemyBrainContext.Transform.position.z;
                    enemyBrainContext.Agent.SetDestination(randomDest);
                }
            }

            UpdateDirectionFromAgentVelocity(ref currentDirection);

            ApplyWallAvoidance(currentDirection, settings.wallDetectDistance, settings.wallLayer);

            RotateTowardsDirection(currentDirection);
        }

        private void TransitionOnLost()
        {
            if (hasSeenPlayer && enemyFSM.HasState(EnemyStateTag.Search))
            {
                TransitionToSearchOrWander(LastKnownPlayerPosition);
            }
            else
            {
                enemyFSM.TransitionTo(EnemyStateTag.Wander);
            }
        }

        public override void Exit()
        {
            if (enemyBrainContext.AlertUI != null) enemyBrainContext.AlertUI.HideAttackWarning();
        }
    }
}
