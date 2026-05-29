using UnityEngine;

namespace Game.Testing
{
    public class EnemyAgent : EnemyNavAgentBase
    {
        [SerializeField] private EnemyBrainProfile brainProfile;

        protected override IEnemyBrain CreateBrain()
        {
            return new EnemyBrain(brainProfile);
        }

        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;
            (ActiveBrain as IBrainHitReaction)?.OnPlayerCollision();
        }
    }
}
