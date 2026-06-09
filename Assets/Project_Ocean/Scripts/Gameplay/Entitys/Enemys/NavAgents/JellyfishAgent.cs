using UnityEngine;

namespace Game.Testing
{
    
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class JellyfishAgent : EnemyAgent
    {
        // NavMesh is not needed — jellyfish moves via Transform directly in JellyfishBrain.
        protected override bool requiresNavMesh => false;

        // ------------------------------------------------------------------ //
        //  Brain factory                                                       //
        // ------------------------------------------------------------------ //
        protected override IEnemyBrain CreateBrain()
        {
            JellyfishProfile profile = BaseProfile as JellyfishProfile;

            if (profile == null)
            {
                Debug.LogError($"[JellyfishAgent] brainProfile must be a JellyfishProfile on {name}.", this);
                return new JellyfishBrain(ScriptableObject.CreateInstance<JellyfishProfile>());
            }

            return new JellyfishBrain(profile);
        }

        // ------------------------------------------------------------------ //
        //  Contact damage                                                      //
        // ------------------------------------------------------------------ //
        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead)
                return;

            JellyfishProfile profile = BaseProfile as JellyfishProfile;
            if (profile == null || player == null)
                return;

            player.TakeDamage(profile.contactDamage);
        }
    }
}
