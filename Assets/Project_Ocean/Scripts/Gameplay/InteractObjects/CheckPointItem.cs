using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    /// Placed in the ocean by the player (F key, Building item). Max 3 active at once.
    /// Pins the rope at this position — rope runs:
    ///   surface → [...wraps...] → checkpoint(pin) → player
    /// Press T to be physically pulled to the most recently placed checkpoint and remove its pin.
    public class CheckPointItem : MonoBehaviour, IItemEffect
    {
        #region Inspector

        [SerializeField] private float pullSpeed    = 15f;
        [SerializeField] private float arrivalDist  = 0.3f;

        #endregion

        #region IItemEffect — runs on the prefab before instantiation

        public bool CanExecute(Player player) => player.CanPlaceCheckpoint;
        public bool KeepInInventory => true;
        public string ActionHint => "T";

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

        /// Physically pull the player toward this checkpoint, then remove the pin.
        public void PullPlayerHere(Player player)
        {
            Vector2 target = transform.position;
            player.StartCoroutine(PullRoutine(player, target, pullSpeed, arrivalDist, this, gameObject));
        }

        private static IEnumerator PullRoutine(Player player, Vector2 target, float speed, float arrivalDist, CheckPointItem checkpoint, GameObject checkpointObj)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) yield break;

            // Apply velocity toward checkpoint each physics step until we arrive
            while (Vector2.Distance(player.transform.position, target) > arrivalDist)
            {
                Vector2 dir = (target - (Vector2)player.transform.position).normalized;
                rb.linearVelocity = dir * speed;
                yield return new WaitForFixedUpdate();
            }

            // Snap to exact position and stop
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            player.transform.position = target;
            rb.bodyType = RigidbodyType2D.Dynamic;

            // Detach and destroy only after the player has fully arrived
            player.OxygenTube.DetachFromCheckpoint(target);
            player.UnregisterCheckpoint(checkpoint);
            Destroy(checkpointObj);
        }

        #endregion
    }
}
