using UnityEngine;

namespace Game.Testing
{
    /// Placed in the ocean by the player (F key, Building item). Max 3 active at once.
    /// Adds a pin to the rope at this position — rope runs:
    ///   surface → [...wraps...] → checkpoint(pin) → player
    /// Press T to teleport to the most recently placed checkpoint and remove its pin.
    public class CheckPointItem : MonoBehaviour, IItemEffect
    {
        #region IItemEffect — runs on the prefab before instantiation

        /// Block placement when 3 checkpoints are already active.
        public bool CanExecute(Player player) => player.CanPlaceCheckpoint;

        public void ExecuteItem(Player player)
        {
            Instantiate(gameObject, player.transform.position, Quaternion.identity);
        }

        #endregion

        #region Placed Instance

        private void Start()
        {
            Player player = GameManager.CurPlayer;
            if (player == null) return;

            player.RegisterCheckpoint(this);
            player.OxygenTube.AttachToCheckpoint(transform.position);
        }

        /// Teleport the player here and remove this pin from the rope.
        /// Always called on the most recently placed checkpoint (LastCheckpoint).
        public void TeleportPlayerHere(Player player)
        {
            // Remove this checkpoint pin (and any wraps added after it) from the rope
            player.OxygenTube.DetachFromCheckpoint(transform.position);

            // Teleport player
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                player.transform.position = transform.position;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                player.transform.position = transform.position;
            }

            player.UnregisterCheckpoint(this);
            Destroy(gameObject);
        }

        #endregion
    }
}
