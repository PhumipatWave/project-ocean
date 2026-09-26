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
            oxygenTube?.SetHealthLimit(CurrentHealth);
        }

        public void ResetToOriginal()
        {
            if (oxygenTube != null)
                oxygenTube.MaxRopeLength = originalMaxRopeLength;

            Configure(originalMaxRopeLength);
            base.Initialize();
            oxygenTube?.SetHealthLimit(CurrentHealth);
        }

        /// Call when a rope upgrade is purchased so ResetToOriginal uses the new max.
        /// Also raises the live max/current health (what the HUD's max-length text reads, and
        /// the rope's HealthLimit) right away, instead of only after the next reset.
        public void IncreaseOriginalMaxLength(float amount)
        {
            originalMaxRopeLength += amount;
            Configure(originalMaxRopeLength);
            Heal(amount); // clamps to the new max and refreshes oxygenTube's HealthLimit
        }

        /// Call when loading a save to sync the snapshot to the saved upgraded value.
        public void SetOriginalMaxLength(float length)
        {
            originalMaxRopeLength = length;
            if (oxygenTube != null)
                oxygenTube.MaxRopeLength = length;
            Configure(length);
            base.Initialize();
            oxygenTube?.SetHealthLimit(CurrentHealth);
        }

        // Damage depletes CurrentHealth only — it no longer shrinks oxygenTube.MaxRopeLength,
        // so the player can always swim out to the full standard/upgraded rope length
        // regardless of how much damage they've taken. Health hitting 0 still pulls the
        // player back to land instead of killing them.
        public override void TakeDamage(float amount)
        {
            if (IsDead) return;

            float remaining = CurrentHealth - amount;

            if (remaining <= 0f)
            {
                SetCurrentHealth(0f);
                oxygenTube?.ResetRope();
                GameManager.Instance.ReturnToLand();
                return;
            }

            SetCurrentHealth(remaining);
            oxygenTube?.SetHealthLimit(remaining);
            FlashHitColor();
            RaiseOnDamaged();
        }

        public override void Heal(float amount)
        {
            base.Heal(amount);
            oxygenTube?.SetHealthLimit(CurrentHealth);
        }
    }
}
