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
