using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Handles 2D sprite flip (on visual child) and body rotation for nav-mesh enemies.
    /// </summary>
    public class EnemyFacingController
    {
        #region FieldProperties
        private Transform body;
        private Transform visualRoot;

        private float turnSmoothTime;
        private float maxTiltAngle;

        private float flipDeadZone;
        private float flipDelay;
        private float flipCooldown;

        private float spriteAngleOffset;
        private float attackAngleThreshold;

        private FacingTurnMode turnMode;
        private float turnSpeed;

        private float turnVelocity;

        private float flipTimer;
        private float flipCooldownTimer;

        private bool suppressRotation;
        private bool facingRight = true;

        public enum FacingTurnMode
        {
            SmoothTime,
            TurnSpeed
        }

        public bool FacingRight => facingRight;
        #endregion

        #region Initialization
        public EnemyFacingController(Transform body, Transform visualRoot, float turnSmoothTime, float maxTiltAngle, float flipDeadZone, float flipDelay, float flipCooldown, float spriteAngleOffset, float attackAngleThreshold, FacingTurnMode turnMode = FacingTurnMode.SmoothTime, float turnSpeed = 5f)
        {
            this.body = body;
            this.visualRoot = visualRoot;
            this.turnSmoothTime = turnSmoothTime;
            this.maxTiltAngle = maxTiltAngle;
            this.flipDeadZone = flipDeadZone;
            this.flipDelay = flipDelay;
            this.flipCooldown = flipCooldown;
            this.spriteAngleOffset = spriteAngleOffset;
            this.attackAngleThreshold = attackAngleThreshold;
            this.turnMode = turnMode;
            this.turnSpeed = turnSpeed;
        }

        public void Initialize()
        {
            visualRoot = CharacterFacing2D.RecheckVisualRoot(body, visualRoot);
            facingRight = CharacterFacing2D.IsFacingRight(body, visualRoot);
        }
        #endregion

        public void SetSuppressRotation(bool suppress)
        {
            suppressRotation = suppress;
        }

        public Vector2 GetFacingDirection()
        {
            Vector2 dir = facingRight ? body.right : -body.right;
            return dir.normalized;
        }

        public void RotateFromVelocity(Vector2 velocity, float minSpeed = 0.01f)
        {
            if (suppressRotation || velocity.sqrMagnitude < minSpeed * minSpeed)
                return;

            RotateTowards(velocity.normalized);
        }

        /// <summary>
        /// Overload that accepts a Vector3 from NavMeshAgent.velocity.
        /// Maps XZ-plane velocity to 2D (X,Y) for games using a 2D NavMesh surface.
        /// Falls back to (X,Y) if the Z component is effectively zero.
        /// </summary>
        public void RotateFromVelocity(Vector3 velocity3d, float minSpeed = 0.01f)
        {
            // Prefer XZ if the agent is on an XZ navmesh (Z is significant)
            Vector2 vel2d = Mathf.Abs(velocity3d.z) > Mathf.Abs(velocity3d.y)
                ? new Vector2(velocity3d.x, velocity3d.z)
                : new Vector2(velocity3d.x, velocity3d.y);

            RotateFromVelocity(vel2d, minSpeed);
        }

        public bool RotateTowards(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.01f)
                return true;

            if (flipCooldownTimer > 0f)
                flipCooldownTimer -= Time.deltaTime;

            bool wantsFaceRight = facingRight;

            if (dir.x > flipDeadZone)
                wantsFaceRight = true;
            else if (dir.x < -flipDeadZone)
                wantsFaceRight = false;

            if (wantsFaceRight != facingRight)
            {
                flipTimer += Time.deltaTime;

                if (flipTimer >= flipDelay && flipCooldownTimer <= 0f)
                {
                    facingRight = wantsFaceRight;
                    flipTimer = 0f;
                    flipCooldownTimer = flipCooldown;
                    // Mirror the current rotation velocity so the tilt doesn't snap
                    turnVelocity = -turnVelocity;
                }
            }
            else
            {
                flipTimer = 0f;
            }

            CharacterFacing2D.ApplyFlip(body, visualRoot, facingRight);

            // Compute tilt relative to the facing side so it stays within [-maxTiltAngle, maxTiltAngle]
            float targetAngle = facingRight
                ? Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg : Mathf.Atan2(-dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;

            targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);
            targetAngle += spriteAngleOffset;

            float smoothDuration = turnMode == FacingTurnMode.TurnSpeed
                ? 1f / Mathf.Max(turnSpeed, 0.01f) : turnSmoothTime;

            // Use body.rotation.eulerAngles mapped to [-180,180] to avoid 0/360 wrap-around
            // that causes SmoothDampAngle to spin the wrong way.
            float currentAngle = Mathf.DeltaAngle(0f, body.eulerAngles.z);
            float smoothAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref turnVelocity, smoothDuration);

            body.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(smoothAngle, targetAngle));
            return angleDifference <= attackAngleThreshold;
        }
    }
}