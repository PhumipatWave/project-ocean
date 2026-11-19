using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public interface IBrainHitReaction
    {
        void OnHit();
        void OnPlayerCollision();
    }
}