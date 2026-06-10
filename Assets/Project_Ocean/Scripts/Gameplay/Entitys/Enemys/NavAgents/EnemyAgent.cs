using UnityEngine;

namespace Game.Testing
{
    public class EnemyAgent : EnemyNavAgentBase
    {
        [SerializeField] private EnemyBaseProfile brainProfile;
        public EnemyBaseProfile BaseProfile { get { return brainProfile; } }

        protected override string GetFishId() => brainProfile != null 
            ? brainProfile.fishId : string.Empty;

        protected override IEnemyBrain CreateBrain()
        {
            return new EnemyBrain(brainProfile);
        }

        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) 
                return;

            (ActiveBrain as IBrainHitReaction)?.OnPlayerCollision();
        }
    }
}
