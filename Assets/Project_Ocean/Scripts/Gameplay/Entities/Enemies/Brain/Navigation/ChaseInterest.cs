using System;

namespace Game.Gameplay
{
    /// <summary>
    /// Decides when a fish gives up chasing an unreachable player, and blocks re-chasing for a while.
    /// A reachable player always bypasses the cooldown.
    /// </summary>
    public sealed class ChaseInterest
    {
        private readonly float unreachableBeforeGiveUp;
        private readonly float giveUpCooldown;
        private readonly Func<float> clock;

        private float unreachableTime;
        private float cooldownEnd = float.NegativeInfinity;

        public ChaseInterest(float unreachableBeforeGiveUp, float giveUpCooldown, Func<float> clock)
        {
            this.unreachableBeforeGiveUp = unreachableBeforeGiveUp;
            this.giveUpCooldown = giveUpCooldown;
            this.clock = clock;
        }

        public bool ShouldGiveUp => unreachableTime >= unreachableBeforeGiveUp;
        public bool IsOnCooldown => clock() < cooldownEnd;

        public void Observe(ReachStatus status, float deltaTime)
        {
            if (status == ReachStatus.Reachable)
                unreachableTime = 0f;
            else
                unreachableTime += deltaTime;
        }

        public bool CanChase(ReachStatus status) => status == ReachStatus.Reachable || !IsOnCooldown;

        public void StartCooldown()
        {
            unreachableTime = 0f;
            cooldownEnd = clock() + giveUpCooldown;
        }
    }
}
