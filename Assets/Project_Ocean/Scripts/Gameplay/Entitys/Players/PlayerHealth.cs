using UnityEngine;

namespace Game.Testing
{
    public class PlayerHealth : EntityHealth
    {
        private PlayerOxygenTube oxygenTube;
        private float originalMaxRopeLength;


        public void SetOxygenTube(PlayerOxygenTube tube)
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


        public override void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (oxygenTube == null) { base.TakeDamage(amount); return; }

            oxygenTube.MaxRopeLength = Mathf.Max(0f, oxygenTube.MaxRopeLength - amount);

            // Rope hit 0 — pull back to land instead of dying
            if (oxygenTube.MaxRopeLength <= 0f)
            {
                oxygenTube.ResetRopeData();
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
