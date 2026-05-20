using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishS Panic state: flees from player / threat at boosted speed.
    /// Broadcasts flee signal to group followers.
    /// Transitions -> Flock when panic timer expires.
    /// </summary>
    public class FishSPanicState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Panic;

        private FishSBrainSettings fishSBrainSetting;
        private FishS_Agent fishSAgent;

        private Vector2 panicDirection;
        private float panicTimer;
        private float hitChaseTimer;

        public FishSPanicState(FishSBrainSettings settings, FishS_Agent host)
        {
            fishSBrainSetting = settings;
            this.fishSAgent = host;
        }

        public void SetFleeSource(Vector2 sourcePos)
        {
            Vector2 away = ((Vector2)enemyBrainContext.Transform.position - sourcePos).normalized;
            float randomAngle = Random.Range(-fishSBrainSetting.panicRandomAngle, fishSBrainSetting.panicRandomAngle);
            panicDirection = EnemyMathHelper.RotateVector(away, randomAngle);
        }

        public void SetHitChaseTimer(float duration) => hitChaseTimer = duration;

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = fishSBrainSetting.speed * fishSBrainSetting.panicSpeedMultiplier;
            panicTimer = fishSBrainSetting.panicDuration;
            BroadcastPanicToFollowers();
        }

        public override void Tick(float deltaTime)
        {
            panicTimer -= deltaTime;
            if (hitChaseTimer > 0f) hitChaseTimer -= deltaTime;

            if (panicTimer <= 0f && hitChaseTimer <= 0f)
            {
                enemyFSM.TransitionTo(EnemyStateTag.Flock);
                return;
            }

            Vector2 moveDir = panicDirection * 3f;

            // Flee from player if still in sight
            if (enemyBrainContext.PlayerTransform != null)
            {
                Vector2 flee = ((Vector2)enemyBrainContext.Transform.position - (Vector2)enemyBrainContext.PlayerTransform.position).normalized;
                moveDir += flee * 2f;
            }

            Vector2 finalDir = moveDir.normalized;
            enemyBrainContext.Agent.SetDestination((Vector3)enemyBrainContext.Transform.position + (Vector3)finalDir * fishSBrainSetting.navDestinationDistance);
            enemyBrainContext.Facing.RotateTowards(finalDir);
        }

        public override void Exit()
        {
            enemyBrainContext.Agent.speed = fishSBrainSetting.speed;
        }

        private void BroadcastPanicToFollowers()
        {
            if (fishSAgent == null || enemyBrainContext.PlayerTransform == null) return;
            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyBrainContext.Transform.position, fishSBrainSetting.groupRadius);
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                FishS_Agent fish = hit.GetComponent<FishS_Agent>();
                if (fish != null && fish != fishSAgent && fish.leader == fishSAgent && fish.groupID == fishSAgent.groupID)
                    fish.FleeFrom(enemyBrainContext.PlayerTransform.position);
            }
        }

        
    }
}