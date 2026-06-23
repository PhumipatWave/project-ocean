using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Agent component for the Crab enemy.
    /// - No NavMesh: crab walks via Rigidbody2D on ground tiles only.
    /// - Turns around on walls and ledges (handled inside CrabBrain).
    /// - Deals contact damage to the player on collision.
    ///
    /// Required on same GameObject: Enemy, Rigidbody2D, Collider2D.
    /// Set Rigidbody2D → Constraints → Freeze Rotation Z = true in the Inspector.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CrabAgent : EnemyAgent
    {
        // Crab moves via Rigidbody2D physics, not NavMesh.
        protected override bool requiresNavMesh => false;

        // ------------------------------------------------------------------ //
        //  Brain factory                                                       //
        // ------------------------------------------------------------------ //
        protected override IEnemyBrain CreateBrain()
        {
            CrabProfile profile = BaseProfile as CrabProfile;

            if (profile == null)
            {
                Debug.LogError(
                    $"[CrabAgent] brainProfile must be a CrabProfile on {name}. " +
                    "Using default values.", this);
                return new CrabBrain(ScriptableObject.CreateInstance<CrabProfile>());
            }

            return new CrabBrain(profile);
        }

        // ------------------------------------------------------------------ //
        //  Contact damage                                                      //
        // ------------------------------------------------------------------ //
        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;

            CrabProfile profile = BaseProfile as CrabProfile;
            if (profile == null || player == null) return;

            CrabBrain crabBrain = ActiveBrain as CrabBrain;
            if (crabBrain != null && !crabBrain.CanDealContactDamage()) return;

            player.TakeDamage(profile.contactDamage);
        }
    }
}
