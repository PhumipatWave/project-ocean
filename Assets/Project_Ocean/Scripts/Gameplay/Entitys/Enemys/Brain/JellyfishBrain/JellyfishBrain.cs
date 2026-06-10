using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Brain for JellyfishEnemy.
    /// Moves only on the Y-axis (bob up/down). No horizontal movement.
    /// Damage is handled by JellyfishAgent on player collision.
    /// </summary>
    public class JellyfishBrain : EnemyBrain
    {
        #region FieldProperties
        private readonly JellyfishProfile jellyfishProfile;

        private Transform selfTransform;
        private FishAlertUI alertUI;

        private float originY;
        private float bobTimer;
        private int bobDirection = 1;

        private bool initialized;
        #endregion

        #region Initialization
        public JellyfishBrain(JellyfishProfile profile) : base(profile)
        {
            jellyfishProfile = profile;
        }

        public override void Initialize(EnemyBrainContext ctx)
        {
            base.Initialize(ctx);

            selfTransform = ctx.Transform;
            alertUI        = ctx.AlertUI;

            originY    = selfTransform.position.y;
            bobTimer   = 0f;
            bobDirection = 1;

            initialized = true;
        }
        #endregion

        public override void Tick(float dt)
        {
            if (!initialized || selfTransform == null)
                return;

            BobVertically(dt);
        }

        #region Private
        private void BobVertically(float dt)
        {
            bobTimer += dt * jellyfishProfile.bobSpeed * bobDirection;

            float newY = originY + Mathf.Sin(bobTimer) * jellyfishProfile.bobAmplitude;

            Vector3 pos = selfTransform.position;
            selfTransform.position = new Vector3(pos.x, newY, pos.z);

            // Flip bob direction at limits so the jellyfish doesn't drift
            if (bobTimer >  Mathf.PI * 2f) bobDirection = -1;
            if (bobTimer < -Mathf.PI * 2f) bobDirection =  1;
        }
        #endregion
    }
}
