using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// FishS Flock/Wander state: boids-style movement + leader following.
    /// Transitions -> Panic when player is spotted or FleeFrom() is called.
    /// </summary>
    public class FishSFlockState : EnemyStateBase
    {
        public override EnemyStateTag StateTag => EnemyStateTag.Flock;

        private FishSBrainSettings fishSBrainSetting;
        private FishS_Agent fishSAgent;
        private List<FishS_Agent> neighbors;

        private Vector2 currentDirection;
        private Vector2 wanderTarget;
        private float wanderTimer;
        private float neighborTimer;

        public Vector2 CurrentDirection => currentDirection;

        public FishSFlockState(FishSBrainSettings settings, FishS_Agent host, List<FishS_Agent> neighbors)
        {
            fishSBrainSetting = settings;
            this.fishSAgent = host;
            this.neighbors = neighbors;
        }

        public override void Enter(EnemyBrainContext ctx, EnemyFSM fsm)
        {
            base.Enter(ctx, fsm);
            enemyBrainContext.Agent.speed = fishSBrainSetting.speed;
            SetNewWanderTarget();
        }

        public override void Tick(float deltaTime)
        {
            neighborTimer -= deltaTime;
            if (neighborTimer <= 0f)
            {
                UpdateNeighbors();
                neighborTimer = fishSBrainSetting.neighborUpdateInterval;
            }

            // Spot player -> Panic
            if (enemyBrainContext.PlayerTransform != null && CheckLineOfSight(enemyBrainContext.PlayerTransform))
            {
                if (enemyFSM.CurrentState is FishSPanicState panic)
                    panic.SetFleeSource(enemyBrainContext.PlayerTransform.position);
                enemyFSM.TransitionTo(EnemyStateTag.Panic);
                return;
            }

            Vector2 moveDir = TickLeaderAndWander(deltaTime);
            moveDir += CalcFlocking();
            moveDir += WallAvoidance();

            if (moveDir.sqrMagnitude > 0.01f)
            {
                currentDirection = moveDir.normalized;
                enemyBrainContext.Agent.SetDestination(
                    (Vector3)enemyBrainContext.Transform.position + (Vector3)currentDirection * fishSBrainSetting.navDestinationDistance
                );
            }

            if (currentDirection.sqrMagnitude > 0.01f)
                enemyBrainContext.Facing.RotateTowards(currentDirection);
        }

        public void FleeFrom(Vector2 source)
        {
            if (enemyFSM.HasState(EnemyStateTag.Panic))
            {
                if (enemyFSM.CurrentState is FishSPanicState panic)
                    panic.SetFleeSource(source);
                enemyFSM.TransitionTo(EnemyStateTag.Panic);
            }
        }

        private Vector2 TickLeaderAndWander(float deltaTime)
        {
            if (fishSAgent == null) return TickWanderDirection(deltaTime);
            if (fishSAgent.leader != null && neighbors.Contains(fishSAgent.leader))
                return ((Vector2)fishSAgent.leader.transform.position - (Vector2)enemyBrainContext.Transform.position).normalized;
            return TickWanderDirection(deltaTime);
        }

        private Vector2 TickWanderDirection(float deltaTime)
        {
            wanderTimer -= deltaTime;
            if (wanderTimer <= 0f || Vector2.Distance(enemyBrainContext.Transform.position, wanderTarget) < 0.5f)
                SetNewWanderTarget();
            return wanderTarget - (Vector2)enemyBrainContext.Transform.position;
        }

        private void SetNewWanderTarget()
        {
            wanderTarget = (Vector2)enemyBrainContext.Transform.position + Random.insideUnitCircle * fishSBrainSetting.wanderRadius;
            wanderTimer = fishSBrainSetting.wanderInterval;
        }

        private Vector2 CalcFlocking()
        {
            if (neighbors.Count == 0) return Vector2.zero;
            Vector2 cohesion = Vector2.zero, separation = Vector2.zero, alignment = Vector2.zero;
            int sepCount = 0;
            foreach (var n in neighbors)
            {
                Vector2 toN = (Vector2)n.transform.position - (Vector2)enemyBrainContext.Transform.position;
                float dist = toN.magnitude;
                cohesion += (Vector2)n.transform.position;
                if (dist < fishSBrainSetting.separationDistance && dist > 0f) { separation += -toN.normalized / dist; sepCount++; }
                alignment += n.MoveDirection;
            }
            Vector2 result = Vector2.zero;
            Vector2 toCentre = cohesion / neighbors.Count - (Vector2)enemyBrainContext.Transform.position;
            if (toCentre.sqrMagnitude > 0.01f) result += toCentre.normalized * fishSBrainSetting.cohesionWeight;
            if (sepCount > 0) result += (separation / sepCount).normalized * fishSBrainSetting.separationWeight;
            Vector2 avgDir = alignment / neighbors.Count;
            if (avgDir.sqrMagnitude > 0.01f) result += avgDir.normalized * fishSBrainSetting.alignmentWeight;
            return result;
        }

        private void UpdateNeighbors()
        {
            neighbors.Clear();
            Collider2D selfCol = enemyBrainContext.Host != null ? enemyBrainContext.Host.GetComponent<Collider2D>() : null;
            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyBrainContext.Transform.position, fishSBrainSetting.groupRadius);
            foreach (var hit in hits)
            {
                if (hit == null || hit == selfCol) continue;
                FishS_Agent fish = hit.GetComponent<FishS_Agent>();
                if (fish != null && fishSAgent != null && fish.groupID == fishSAgent.groupID)
                    neighbors.Add(fish);
            }
        }

        private bool CheckLineOfSight(Transform target) =>
            EnemyVisionHelper.HasLineOfSightRay(
                enemyBrainContext.Transform.position, target.position, currentDirection,
                fishSBrainSetting.viewDistance, fishSBrainSetting.viewAngle, fishSBrainSetting.wallLayer, target
            );

        private Vector2 WallAvoidance()
        {
            if (currentDirection.sqrMagnitude < 0.01f) return Vector2.zero;
            Vector2 avoid = Vector2.zero;
            float detectDist = fishSBrainSetting.wallDetectDistance * (enemyBrainContext.Agent.speed / fishSBrainSetting.speed);
            float[] angles = { 0f, 45f, 90f, 135f, 180f, -45f, -90f, -135f };
            foreach (float a in angles)
            {
                Vector2 dir = RotateVector(currentDirection, a);
                RaycastHit2D hit = Physics2D.Raycast(enemyBrainContext.Transform.position, dir, detectDist, fishSBrainSetting.wallLayer);
                if (hit.collider == null) continue;
                avoid += hit.normal * (1f - hit.distance / detectDist);
            }
            return avoid.sqrMagnitude > 0.01f ? avoid.normalized * fishSBrainSetting.wallAvoidWeight : Vector2.zero;
        }

        private static Vector2 RotateVector(Vector2 v, float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
        }
    }
}