using UnityEngine;

namespace Game.Testing
{
    public class LinearPatrolBrain : IEnemyBrain, IBrainHitReaction
    {
        private enum State { Moving, Pausing }

        private readonly LinearPatrolProfile profile;
        private EnemyBrainContext ctx;
        private Rigidbody2D rb;

        private State state = State.Moving;
        private float direction = 1f;
        private float pauseTimer;
        private Vector2 homePosition;
        private float lastContactDamageTime = float.MinValue;

        public bool IsAttacking => false;

        public LinearPatrolBrain(LinearPatrolProfile profile)
        {
            this.profile = profile;
        }

        public void Initialize(EnemyBrainContext context)
        {
            ctx = context;
            rb = context.Owner.GetComponent<Rigidbody2D>();
            homePosition = context.Transform.position;
            direction = 1f;
            state = State.Moving;
            pauseTimer = 0f;
            lastContactDamageTime = float.MinValue;
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
            if (profile.contactDamage <= 0f) return;
            if (Time.time - lastContactDamageTime < profile.contactDamageCooldown) return;
            lastContactDamageTime = Time.time;
            ctx?.Player?.TakeDamage(profile.contactDamage);
        }

        private void HandleMove(float deltaTime)
        {
            bool shouldReverse = profile.movementMode == PatrolMovementMode.UntilWall
                ? (IsWallAhead() || IsEdgeAhead())
                : HasReachedLimit();

            if (shouldReverse) { EnterPause(); return; }

            ApplyMovement(deltaTime);
        }

        private void ApplyMovement(float deltaTime)
        {
            if (profile.axis == PatrolAxis.Horizontal)
            {
                if (rb != null)
                    rb.linearVelocity = new Vector2(direction * profile.speed, rb.linearVelocity.y);
            }
            else
            {
                ctx.Transform.position += new Vector3(0f, direction * profile.speed * deltaTime, 0f);
            }
        }

        private bool HasReachedLimit()
        {
            Vector2 pos = ctx.Transform.position;
            float dist = profile.axis == PatrolAxis.Horizontal
                ? Mathf.Abs(pos.x - homePosition.x)
                : Mathf.Abs(pos.y - homePosition.y);
            return dist >= profile.moveLength;
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

        private void HandlePause(float deltaTime)
        {
            if (rb != null && profile.axis == PatrolAxis.Horizontal)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            pauseTimer -= deltaTime;
            if (pauseTimer <= 0f)
            {
                direction *= -1f;
                homePosition = ctx.Transform.position; // reset origin for FixedLength
                state = State.Moving;
            }
        }

        private void EnterPause()
        {
            state = State.Pausing;
            pauseTimer = profile.pauseDuration;
        }

        private void UpdateFacing()
        {
            if (profile.axis != PatrolAxis.Horizontal) return;
            ctx.Facing?.RotateTowards(new Vector2(direction, 0f));
        }
    }
}
