using UnityEngine;

namespace Game.Testing
{
    public interface IBrainHitReaction
    {
        void OnBulletHit();
        void OnPlayerCollision();
    }
}