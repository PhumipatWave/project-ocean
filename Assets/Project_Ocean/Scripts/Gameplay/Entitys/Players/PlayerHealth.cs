using UnityEngine;

namespace Game.Testing
{
    public class PlayerHealth : EntityHealth
    {
        private OxygenRope oxygenTube;
        private float originalMaxRopeLength;

        public void SetOxygenRope(OxygenRope tube)
        {
            oxygenTube = tube;
        }

        public override void Initialize()
        {
            if (oxygenTube != null)
            {
                // Only snapshot the original length the very first time
                if (originalMaxRopeLength <= 0f)
                    originalMaxRopeLength = oxygenTube.MaxRopeLength;

                Configure(originalMaxRopeLength);
            }

            base.Initialize();
        }

        public void ResetToOriginal()
        {
            if (oxygenTube != null)
                oxygenTube.MaxRopeLength = originalMaxRopeLength;

            Configure(originalMaxRopeLength);
            base.Initialize();
        }

        /// Call when a rope upgrade is purchased so ResetToOriginal uses the new max.
        public void IncreaseOriginalMaxLength(float amount)
        {
            originalMaxRopeLength += amount;
        }

        /// Call when loading a save to sync the snapshot to the saved upgraded value.
        public void SetOriginalMaxLength(float length)
        {
            originalMaxRopeLength = length;
            if (oxygenTube != null)
                oxygenTube.MaxRopeLength = length;
            Configure(length);
            base.Initialize();
        }

        public override void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (oxygenTube == null) 
            { 
                base.TakeDamage(amount); 
                return; 
            }

            oxygenTube.MaxRopeLength = Mathf.Max(0f, oxygenTube.MaxRopeLength - amount);

            // Rope hit 0 — pull back to land instead of dying
            if (oxygenTube.MaxRopeLength <= 0f)
            {
                oxygenTube.ResetRope();
                GameManager.Instance.ReturnToLand();
                return;
            }

            // Sync maxHealth and currentHealth to the new rope length
            Configure(oxygenTube.MaxRopeLength);
            Initialize();

            FlashHitColor();
            RaiseOnDamaged();
        }

        public override void Heal(float amount)
        {
            base.Heal(amount);

            // Sync rope length up to match restored health
            if (oxygenTube != null)
                oxygenTube.MaxRopeLength = CurrentHealth;
        }
    }
}
