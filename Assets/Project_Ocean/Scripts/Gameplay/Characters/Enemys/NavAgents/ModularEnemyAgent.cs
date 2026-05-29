using UnityEngine;

namespace Game.Testing
{
    public class ModularEnemyAgent : EnemyNavAgentBase
    {
        [SerializeField] private EnemyBrainProfile brainProfile;

        private ModularEnemyBrain modularBrain;

        protected override IEnemyBrain CreateBrain()
        {
            modularBrain = new ModularEnemyBrain(brainProfile);
            return modularBrain;
        }

        protected override void HandleBulletHit(GameObject bullet)
        {
            if (isEnemyDead) return;
            if (bullet == null || !bullet.CompareTag(playerBulletTag)) return;

            (modularBrain as IBrainHitReaction)?.OnBulletHit();
        }

        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;
            (modularBrain as IBrainHitReaction)?.OnPlayerCollision();
        }
    }
}
