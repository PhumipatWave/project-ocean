using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class LinearPatrolBrain : IEnemyBrain, IBrainHitReaction
    {
        private enum State { Moving, Pausing }

        // Random first-move delay (randomness > 0) so enemies spawned together don't start in sync.
        private const float MaxStartDelay = 1.5f;
        private const float MinDriftFrequency = 0.25f;
        private const float MaxDriftFrequency = 0.6f;

        private readonly LinearPatrolProfile profile;
        private EnemyBrainContext ctx;
        private Rigidbody2D rb;
        private Collider2D bodyCollider;

        private State state = State.Moving;
        private float direction = 1f;
        private float pauseTimer;
        private Vector2 homePosition;
        private float lastContactDamageTime = float.MinValue;

        // Current leg's values, re-rolled every leg when randomness > 0.
        private float legSpeed;
        private float legLength;
        private float legPause;

        private float driftPhase;
        private float driftFrequency;

        public bool IsAttacking => false;

        public LinearPatrolBrain(LinearPatrolProfile profile)
        {
            this.profile = profile;
        }

        public void Initialize(EnemyBrainContext context)
        {
            ctx = context;
            rb = context.Owner.GetComponent<Rigidbody2D>();
            bodyCollider = FindBodyCollider(context.Owner);
            homePosition = context.Transform.position;
            direction = 1f;
            state = State.Moving;
            pauseTimer = 0f;
            lastContactDamageTime = float.MinValue;

            RollLeg();

            if (profile.randomness > 0f)
            {
                // Start paused for a random moment; the pause ends by flipping direction, so this is a random start direction too.
                direction = Random.value < 0.5f ? 1f : -1f;
                state = State.Pausing;
                pauseTimer = Random.Range(0f, MaxStartDelay);
            }

            driftPhase = Random.Range(0f, Mathf.PI * 2f);
            driftFrequency = Random.Range(MinDriftFrequency, MaxDriftFrequency);

            var hitbox = context.Owner.GetComponentInChildren<EnemyAttackHitbox>(true);
            hitbox?.Initialize(context.Owner, profile.attackDamage);
        }

        public void Tick(float deltaTime)
        {
            if (ctx == null || ctx.IsDead) return;

            switch (state)
            {
                case State.Moving:  HandleMove(deltaTime);  break;
                case State.Pausing: HandlePause(deltaTime); break;
            }

            UpdateFacing();
        }

        // IEnemyBrain — hit by bullet: flinch
        public void OnHit(Transform attacker) => EnterPause();

        public void OnDeath()
        {
            if (rb != null && profile.axis == PatrolAxis.Horizontal)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // IBrainHitReaction — hit by damage or player contact
        public void OnHit() => EnterPause();

        public void OnPlayerCollision()
        {
            // contact damage disabled — using animation-driven hitbox instead
            // if (profile.contactDamage <= 0f) return;
            // if (Time.time - lastContactDamageTime < profile.contactDamageCooldown) return;
            // lastContactDamageTime = Time.time;
            // ctx?.Player?.TakeDamage(profile.contactDamage);
        }

        private void HandleMove(float deltaTime)
        {
            bool shouldReverse = profile.movementMode == PatrolMovementMode.UntilWall
                ? (IsWallAhead() || IsEdgeAhead())
                : HasReachedLimit();

            // Vertical patrol had no wall/ground check in FixedLength mode: turn around at ceilings and the sea floor.
            if (profile.axis == PatrolAxis.Vertical && IsBlocked(new Vector2(0f, direction)))
                shouldReverse = true;

            if (shouldReverse) { EnterPause(); return; }

            ApplyMovement(deltaTime);
            ApplyDrift(deltaTime);
        }

        private void ApplyMovement(float deltaTime)
        {
            if (profile.axis == PatrolAxis.Horizontal)
            {
                if (rb != null)
                    rb.linearVelocity = new Vector2(direction * legSpeed, rb.linearVelocity.y);
            }
            else
            {
                ctx.Transform.position += new Vector3(0f, direction * legSpeed * deltaTime, 0f);
            }
        }

        // Side-to-side sway for vertical patrollers, each at its own rhythm. Skipped when a wall is that way.
        private void ApplyDrift(float deltaTime)
        {
            if (profile.axis != PatrolAxis.Vertical || profile.driftAmplitude <= 0f)
                return;

            float previous = Mathf.Sin(driftPhase);
            driftPhase += deltaTime * driftFrequency * Mathf.PI * 2f;
            float dx = profile.driftAmplitude * (Mathf.Sin(driftPhase) - previous);

            if (Mathf.Approximately(dx, 0f) || IsBlocked(new Vector2(Mathf.Sign(dx), 0f)))
                return;

            ctx.Transform.position += new Vector3(dx, 0f, 0f);
        }

        private bool HasReachedLimit()
        {
            Vector2 pos = ctx.Transform.position;
            float dist = profile.axis == PatrolAxis.Horizontal
                ? Mathf.Abs(pos.x - homePosition.x)
                : Mathf.Abs(pos.y - homePosition.y);
            return dist >= legLength;
        }

        private bool IsWallAhead()
        {
            Vector2 rayDir = profile.axis == PatrolAxis.Horizontal
                ? new Vector2(direction, 0f)
                : new Vector2(0f, direction);
            return Physics2D.Raycast(
                ctx.Transform.position, rayDir,
                profile.wallDetectDistance, profile.wallLayer
            ).collider != null;
        }

        private bool IsEdgeAhead()
        {
            if (profile.axis != PatrolAxis.Horizontal) return false;
            var origin = (Vector2)ctx.Transform.position + new Vector2(direction * profile.groundCheckOffset, 0f);
            var mask = profile.groundLayer.value != 0 ? profile.groundLayer : profile.wallLayer;
            return Physics2D.Raycast(origin, Vector2.down, profile.groundCheckDistance, mask).collider == null;
        }

        // Wall or ground within wallDetectDistance of the body's edge in an axis direction.
        // Measured from the collider edge (not the pivot) so big sprites don't clip into walls.
        private bool IsBlocked(Vector2 axisDirection)
        {
            Vector2 origin = ctx.Transform.position;
            float edge = 0f;

            if (bodyCollider != null)
            {
                Bounds bounds = bodyCollider.bounds;
                origin = bounds.center;
                edge = axisDirection.x != 0f ? bounds.extents.x : bounds.extents.y;
            }

            int mask = profile.wallLayer.value | profile.groundLayer.value;
            return Physics2D.Raycast(origin, axisDirection, edge + profile.wallDetectDistance, mask).collider != null;
        }

        private void HandlePause(float deltaTime)
        {
            if (rb != null && profile.axis == PatrolAxis.Horizontal)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            pauseTimer -= deltaTime;
            if (pauseTimer <= 0f)
            {
                direction *= -1f;
                homePosition = ctx.Transform.position; // reset origin for FixedLength
                RollLeg();
                state = State.Moving;
            }
        }

        private void EnterPause()
        {
            state = State.Pausing;
            pauseTimer = legPause;
        }

        // randomness 0 keeps the exact profile values.
        private void RollLeg()
        {
            legSpeed = profile.speed * Vary();
            legLength = profile.moveLength * Vary();
            legPause = profile.pauseDuration * Vary();
        }

        private float Vary() => 1f + Random.Range(-profile.randomness, profile.randomness);

        private static Collider2D FindBodyCollider(Component owner)
        {
            foreach (var col in owner.GetComponents<Collider2D>())
            {
                if (!col.isTrigger)
                    return col;
            }

            return null;
        }

        private void UpdateFacing()
        {
            if (profile.axis != PatrolAxis.Horizontal) return;
            ctx.Facing?.RotateTowards(new Vector2(direction, 0f));
        }
    }
}
