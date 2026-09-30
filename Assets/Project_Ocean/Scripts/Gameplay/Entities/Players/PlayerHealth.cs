using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class PlayerHealth : EntityHealth, ISaveParticipant
    {
        [Tooltip("Seconds of invincibility after taking a hit (scaled time, so aim slow-mo stretches it).")]
        [SerializeField, Min(0f)] private float invincibleDuration = 0.75f;

        private OxygenRope oxygenTube;
        private float originalMaxRopeLength;
        private float invincibleUntil;

        public bool IsInvincible => Time.time < invincibleUntil;

        private void OnEnable() => UpgradeService.OnLevelGranted += HandleLevelGranted;
        private void OnDisable() => UpgradeService.OnLevelGranted -= HandleLevelGranted;

        private void HandleLevelGranted(UpgradeData data)
        {
            if (data.type != UpgradeType.Rope || oxygenTube == null) return;
            oxygenTube.IncreaseMaxLength(data.value);
            IncreaseOriginalMaxLength(data.value);
        }

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
            invincibleUntil = 0f;
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

        /// <summary>Raises the base max length after a rope upgrade, and the live max right away.</summary>
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

        // Damage lowers health only, never the rope length; at 0 the player is pulled back to land, not killed.
        public override void TakeDamage(float amount)
        {
            if (IsDead || IsInvincible) return;

            float remaining = CurrentHealth - amount;

            if (remaining <= 0f)
            {
                SetCurrentHealth(0f);
                oxygenTube?.ResetRope();
                GameFlowEvents.RequestReturnToLand();
                return;
            }

            SetCurrentHealth(remaining);
            invincibleUntil = Time.time + invincibleDuration;
            oxygenTube?.SetHealthLimit(remaining);
            FlashHitColor();
            RaiseOnDamaged();
        }

        public override void Heal(float amount)
        {
            base.Heal(amount);
            oxygenTube?.SetHealthLimit(CurrentHealth);
        }

        public void Collect(SaveData data)
        {
            if (oxygenTube != null)
                data.maxRopeLength = oxygenTube.MaxRopeLength;
        }

        // SetOriginalMaxLength syncs both the rope and this snapshot, so ResetToOriginal()
        // after the next dive restores the saved upgraded max.
        public void Apply(SaveData data)
        {
            if (oxygenTube != null)
                SetOriginalMaxLength(data.maxRopeLength);
        }
    }
}
