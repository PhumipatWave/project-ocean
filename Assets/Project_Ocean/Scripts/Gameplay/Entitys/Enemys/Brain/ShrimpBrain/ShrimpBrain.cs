using UnityEngine;
using UnityEngine.AI;

namespace Game.Testing
{
    /// <summary>
    /// Brain for the Shrimp enemy.
    /// Implements IEnemyBrain — receives context from EnemyNavAgentBase.
    ///
    /// Behaviour:
    ///   Patrol → เดินซ้าย-ขวาในระยะ patrolRange จากจุด spawn
    ///   Dead   → all ticks ignored
    ///
    /// Contact damage is triggered externally via OnPlayerContact()
    /// called from ShrimpAgent.HandlePlayerCollision().
    /// </summary>
    public class ShrimpBrain : IEnemyBrain
    {
        // ------------------------------------------------------------------ //
        //  State machine                                                       //
        // ------------------------------------------------------------------ //
        private enum State { Patrol, Dead }
        private State state = State.Patrol;

        // ------------------------------------------------------------------ //
        //  IEnemyBrain — context refs (set in Initialize)                     //
        // ------------------------------------------------------------------ //
        private ShrimpProfile profile;
        private Enemy          enemy;
        private Transform      selfTransform;
        private NavMeshAgent   navAgent;

        // ------------------------------------------------------------------ //
        //  Patrol                                                              //
        // ------------------------------------------------------------------ //
        private Vector3 homePosition;
        private bool    goingRight    = true;
        private float   turnDelayTimer = 0f;
        private bool    waiting        = false;

        // ------------------------------------------------------------------ //
        //  Contact damage                                                      //
        // ------------------------------------------------------------------ //
        private float damageCooldownTimer;

        // ------------------------------------------------------------------ //
        //  IEnemyBrain.IsAttacking                                            //
        // ------------------------------------------------------------------ //
        public bool IsAttacking => false;

        // ------------------------------------------------------------------ //
        //  Constructor                                                         //
        // ------------------------------------------------------------------ //
        public ShrimpBrain(ShrimpProfile profile)
        {
            this.profile = profile;
        }

        // ------------------------------------------------------------------ //
        //  IEnemyBrain.Initialize                                             //
        // ------------------------------------------------------------------ //
        public void Initialize(EnemyBrainContext context)
        {
            enemy         = context.Owner;
            selfTransform = context.Transform;
            navAgent      = context.Agent;

            if (navAgent != null)
            {
                navAgent.speed            = profile != null ? profile.speed : 2.5f;
                navAgent.stoppingDistance = 0.1f;
            }

            homePosition       = selfTransform.position;
            goingRight         = true;
            waiting            = false;
            turnDelayTimer     = 0f;
            state              = State.Patrol;
            damageCooldownTimer = 0f;

            SetPatrolTarget();
        }

        // ------------------------------------------------------------------ //
        //  IEnemyBrain.Tick                                                   //
        // ------------------------------------------------------------------ //
        public void Tick(float deltaTime)
        {
            if (state == State.Dead) return;

            // Cooldown timer
            if (damageCooldownTimer > 0f)
                damageCooldownTimer -= deltaTime;

            // รอก่อนกลับทิศ
            if (waiting)
            {
                turnDelayTimer -= deltaTime;
                if (turnDelayTimer <= 0f)
                {
                    waiting    = false;
                    goingRight = !goingRight;
                    SetPatrolTarget();
                }
                return;
            }

            // ถึงเป้าหมายแล้ว → เริ่มรอกลับทิศ
            if (navAgent != null && !navAgent.pathPending &&
                navAgent.remainingDistance <= navAgent.stoppingDistance + 0.05f)
            {
                waiting        = true;
                turnDelayTimer = profile != null ? profile.patrolTurnDelay : 0.3f;
            }
        }

        // ------------------------------------------------------------------ //
        //  IEnemyBrain.OnHit                                                  //
        // ------------------------------------------------------------------ //
        public void OnHit(Transform attacker)
        {
            // กุ้งไม่ไล่ตาม — แค่เดิน patrol ต่อ
        }

        // ------------------------------------------------------------------ //
        //  IEnemyBrain.OnDeath                                                //
        // ------------------------------------------------------------------ //
        public void OnDeath()
        {
            state = State.Dead;

            if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
            }
        }

        // ------------------------------------------------------------------ //
        //  Contact damage — called by ShrimpAgent.HandlePlayerCollision()     //
        // ------------------------------------------------------------------ //
        public void OnPlayerContact(Player player)
        {
            if (state == State.Dead)             return;
            if (damageCooldownTimer > 0f)        return;
            if (profile == null || player == null) return;

            player.TakeDamage(profile.contactDamage);
            damageCooldownTimer = profile.damageCooldown;
        }

        // ------------------------------------------------------------------ //
        //  Helpers                                                             //
        // ------------------------------------------------------------------ //
        private void SetPatrolTarget()
        {
            if (navAgent == null || !navAgent.isOnNavMesh) return;

            float   targetX = homePosition.x + (goingRight ? profile.patrolRange : -profile.patrolRange);
            Vector3 target  = new Vector3(targetX, homePosition.y, homePosition.z);
            navAgent.isStopped = false;
            navAgent.SetDestination(target);
        }
    }
}
