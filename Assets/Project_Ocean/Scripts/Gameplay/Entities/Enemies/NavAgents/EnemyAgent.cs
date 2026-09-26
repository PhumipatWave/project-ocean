using UnityEngine;

namespace Game.Testing
{
    public class EnemyAgent : EnemyNavAgentBase
    {
        [SerializeField] private EnemyBaseProfile brainProfile;
        public EnemyBaseProfile BaseProfile => brainProfile;

        protected override bool requiresNavMesh => brainProfile?.RequiresNavMesh ?? true;
        public override float AutoReturnDelay => brainProfile?.autoReturnDelay ?? 60f;

        protected override string GetFishId() => brainProfile != null
            ? brainProfile.fishId : string.Empty;

        protected override IEnemyBrain CreateBrain()
        {
            if (brainProfile == null)
            {
                Debug.LogError($"[EnemyAgent] No brainProfile assigned on {name}.", this);
                return new EnemyBrain(ScriptableObject.CreateInstance<AggressiveEnemyProfile>());
            }
            return brainProfile.CreateBrain();
        }

        protected override void OnTick()
        {
            if (brainProfile != null && brainProfile.tickFacingFromVelocity)
                TickFacingFromAgentVelocity();
        }

        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;
            (ActiveBrain as IBrainHitReaction)?.OnPlayerCollision();
        }
    }
}
