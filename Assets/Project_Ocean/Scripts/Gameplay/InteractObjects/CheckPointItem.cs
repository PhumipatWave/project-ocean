using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Attach this to the CheckPoint prefab.
    /// F key (via Player.ExecuteItem) instantiates the prefab - Start() self-registers with oxygenTube.
    /// T key calls oxygenTube.TeleportToCheckPoint() directly on Player (free action).
    /// </summary>
    public class CheckPointItem : MonoBehaviour, IItemEffect
    {
        private PlayerOxygenTube oxygenTube;

        /// <summary>
        /// Called by Player.ExecuteItem() when the CheckPoint item is consumed (F key).
        /// Instantiates this prefab at the player's current position.
        /// The spawned instance's Start() then self-registers the checkpoint.
        /// </summary>
public void ExecuteItem(Player player)
        {
            Instantiate(gameObject, player.transform.position, Quaternion.identity);
        }

        private void Start()
        {
            oxygenTube = GameManager.CurPlayer.GetComponentInChildren<PlayerOxygenTube>();

            if (oxygenTube == null)
            {
                Debug.LogWarning("[CheckPointItem] Could not find PlayerOxygenTube on CurPlayer.");
                return;
            }

            oxygenTube.SpawnCheckPoint(this.transform);
            Debug.Log($"[CheckPointItem] Checkpoint registered at {transform.position}");
        }
    

/// <summary>
        /// Checked BEFORE the item is consumed from inventory.
        /// Returns false if checkpoint limit is already reached - item is NOT consumed.
        /// </summary>
        public bool CanExecute(Player player)
        {
            if (player.OxygenTube == null)
            {
                Debug.LogWarning("[CheckPointItem] Player has no OxygenTube.");
                return false;
            }

            if (player.OxygenTube.IsCheckpointAtMax())
            {
                Debug.Log("[CheckPointItem] Max checkpoints reached - item not consumed.");
                return false;
            }

            return true;
        }
}
}
