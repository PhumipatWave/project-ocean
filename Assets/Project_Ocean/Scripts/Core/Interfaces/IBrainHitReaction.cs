using UnityEngine;

namespace Game.Testing
{
    public interface IBrainHitReaction
    {
        void OnHit();
        void OnPlayerCollision();
    }
}