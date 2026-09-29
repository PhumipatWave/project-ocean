// Checkpoint item is unused in the game (only MiniHub is). To re-enable: move this folder's files back
// (script → Gameplay/Items/Placeables) and add ENABLE_CHECKPOINT_ITEM to Scripting Define Symbols.
#if ENABLE_CHECKPOINT_ITEM
using System.Collections;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// Placed in the ocean by the player (F key, Building item). Max 3 active at once.
    /// Pins the rope at this position — rope runs:
    ///   surface → [...wraps...] → checkpoint(pin) → player
    /// Press T to be physically pulled to the most recently placed checkpoint and remove its pin.
    public class CheckPointItem : MonoBehaviour, IItemEffect
    {
        #region Inspector

        [SerializeField] private float pullSpeed     = 15f;
        [SerializeField] private float arrivalDist   = 0.3f;
        [SerializeField] private float timeoutMult   = 3f;   // maxTime = distance/speed * this
        [SerializeField] private LineRenderer pullLine;       // assign in Inspector (optional)

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (pullLine == null)
                pullLine = GetComponent<LineRenderer>();

            if (pullLine != null)
            {
                pullLine.positionCount = 2;
                pullLine.enabled = false;
            }
        }

        #endregion

        #region IItemEffect — runs on the prefab before instantiation

        public bool CanExecute(Player player)
        {
            int slot = Inventory.Current?.ActiveBuildingSlotIndex ?? -1;
            return player.CanPlaceCheckpointFromSlot(slot);
        }
        public bool KeepInInventory => true;      // slot visual stays until checkpoint is used (T key)
        public string ActionHint => string.Empty; // T hint driven by RefreshHintUI when checkpoints are active

        public void ExecuteItem(Player player)
        {
            Instantiate(gameObject, player.transform.position, Quaternion.identity);
        }

        #endregion

        #region Placed Instance

        /// The building slot index that placed this checkpoint — used to clear the slot visual on use.
        public int SourceSlotIndex { get; private set; } = -1;

        private void Start()
        {
            Player player = PlayerRegistry.Local;
            if (player == null) return;

            SourceSlotIndex = Inventory.Current?.ActiveBuildingSlotIndex ?? -1;
            player.RegisterCheckpoint(this);
            player.OxygenTube.AttachToCheckpoint(transform.position);
        }

        /// Physically pull the player toward this checkpoint, then remove the pin.
        public void PullPlayerHere(Player player)
        {
            player.StartPull(PullRoutine(player));
        }

        private IEnumerator PullRoutine(Player player)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) yield break;

            Vector2 target  = transform.position;
            float   dist0   = Vector2.Distance(player.transform.position, target);
            float   maxTime = Mathf.Max(2f, dist0 / pullSpeed * timeoutMult);
            float   elapsed = 0f;

            if (pullLine != null) pullLine.enabled = true;

            try
            {
                while (this != null &&
                       Vector2.Distance(player.transform.position, target) > arrivalDist &&
                       elapsed < maxTime)
                {
                    if (pullLine != null)
                    {
                        pullLine.SetPosition(0, transform.position);
                        pullLine.SetPosition(1, player.transform.position);
                    }

                    rb.linearVelocity = (target - (Vector2)player.transform.position).normalized * pullSpeed;
                    elapsed += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
            finally
            {
                // Runs on success, timeout, and StopCoroutine (scene transition)
                if (pullLine != null) pullLine.enabled = false;
                rb.linearVelocity = Vector2.zero;
            }

            // Bail out if the checkpoint was destroyed externally (e.g. scene unloading)
            if (this == null) yield break;

            bool arrived = Vector2.Distance(player.transform.position, target) <= arrivalDist;
            if (arrived)
            {
                // Snap to exact target position
                rb.bodyType = RigidbodyType2D.Kinematic;
                player.transform.position = target;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            // Clean up regardless of whether we fully arrived or timed out
            player.OxygenTube.DetachFromCheckpoint(target);
            player.UnregisterCheckpoint(this);
            Destroy(gameObject);
        }

        #endregion
    }
}
#endif
