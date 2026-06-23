using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Brain for the Crab enemy.
    /// Walks left/right on solid ground only.
    /// Turns around when it detects a wall ahead OR no ground ahead (ledge).
    /// Cannot fly — movement is purely horizontal via Rigidbody2D velocity.
    /// </summary>
    public class CrabBrain : IEnemyBrain, IBrainHitReaction
    {
        // ------------------------------------------------------------------ //
        //  State machine                                                       //
        // ------------------------------------------------------------------ //
        private enum CrabState { Walking, Pausing }
        private CrabState state = CrabState.Walking;

        // ------------------------------------------------------------------ //
        //  References (filled in Initialize)                                  //
        // ------------------------------------------------------------------ //
        private readonly CrabProfile profile;
        private EnemyBrainContext ctx;
        private Rigidbody2D rb;

        // ------------------------------------------------------------------ //
        //  Runtime fields                                                      //
        // ------------------------------------------------------------------ //
        private float facingDir = 1f;   // +1 = right, -1 = left
        private float pauseTimer = 0f;
        private float damageCooldownTimer = 0f;

        // IEnemyBrain
        public bool IsAttacking => false;

        // ------------------------------------------------------------------ //
        //  Constructor                                                         //
        // ------------------------------------------------------------------ //
        public CrabBrain(CrabProfile profile)
        {
            this.profile = profile;
        }

        // ------------------------------------------------------------------ //
        //  IEnemyBrain                                                         //
        // ------------------------------------------------------------------ //
        public void Initialize(EnemyBrainContext context)
        {
            ctx = context;
            rb  = context.Owner.GetComponent<Rigidbody2D>();

            if (rb == null)
                Debug.LogError("[CrabBrain] Rigidbody2D not found on crab GameObject.", context.Owner);

            // Ensure gravity is on so the crab stays grounded.
            if (rb != null)
                rb.gravityScale = 1f;

            facingDir = 1f;
        }

        public void Tick(float deltaTime)
        {
            if (ctx == null || ctx.IsDead || rb == null) return;

            damageCooldownTimer -= deltaTime;

            switch (state)
            {
                case CrabState.Pausing:
                    HandlePause(deltaTime);
                    break;

                case CrabState.Walking:
                    HandleWalk();
                    break;
            }

            // Flip sprite to match facing direction.
            ctx.Facing?.RotateTowards(new Vector2(facingDir, 0f));
        }

        public void OnHit(Transform attacker)
        {
            // Flinch: brief pause then continue walking.
            EnterPause();
        }

        public void OnDeath()
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // ------------------------------------------------------------------ //
        //  IBrainHitReaction                                                   //
        // ------------------------------------------------------------------ //
        public void OnHit()
        {
            // IBrainHitReaction.OnHit() — no attacker info needed; just flinch.
            EnterPause();
        }

        public void OnPlayerCollision()
        {
            // Damage is dealt by CrabAgent.HandlePlayerCollision.
        }

        // ------------------------------------------------------------------ //
        //  Internal helpers                                                    //
        // ------------------------------------------------------------------ //
        private void HandleWalk()
        {
            if (IsWallAhead() || IsLedgeAhead())
            {
                EnterPause();
                return;
            }

            // Keep vertical velocity (gravity) intact; only drive horizontal.
            rb.linearVelocity = new Vector2(facingDir * profile.walkSpeed, rb.linearVelocity.y);
        }

        private void HandlePause(float deltaTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            pauseTimer -= deltaTime;
            if (pauseTimer <= 0f)
            {
                facingDir *= -1f;
                state = CrabState.Walking;
            }
        }

        private void EnterPause()
        {
            state      = CrabState.Pausing;
            pauseTimer = profile.turnPauseDuration;
        }

        /// <summary>True when a wall collider is directly ahead.</summary>
        private bool IsWallAhead()
        {
            Vector2 origin    = (Vector2)ctx.Transform.position;
            Vector2 direction = new Vector2(facingDir, 0f);

            RaycastHit2D hit = Physics2D.Raycast(
                origin, direction,
                profile.wallCheckDistance,
                profile.groundLayer);

            return hit.collider != null;
        }

        /// <summary>True when there is no ground ahead (ledge).</summary>
        private bool IsLedgeAhead()
        {
            Vector2 origin = (Vector2)ctx.Transform.position
                             + new Vector2(facingDir * profile.edgeCheckDistance, 0f);

            RaycastHit2D hit = Physics2D.Raycast(
                origin, Vector2.down,
                profile.edgeCheckDistance * 2f,
                profile.groundLayer);

            return hit.collider == null;
        }

        // ------------------------------------------------------------------ //
        //  Damage cooldown accessor (used by CrabAgent)                       //
        // ------------------------------------------------------------------ //
        public bool CanDealContactDamage()
        {
            if (damageCooldownTimer > 0f) return false;
            damageCooldownTimer = profile.contactDamageCooldown;
            return true;
        }
    }
}
