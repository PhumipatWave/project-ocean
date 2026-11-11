using UnityEngine;

namespace Game.Testing
{
    public class CheckPointItem : MonoBehaviour, IItemEffect
    {
        private PlayerOxygenTube oxygenTube;

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
