using UnityEngine;

namespace Game.Testing
{
    public class EnemyLoot : MonoBehaviour
    {
        [SerializeField] private Item dropItem;

        public void CollectByPlayer()
        {
            if (GameManager.CurPlayer != null && GameManager.CurPlayer.Audio != null)
                GameManager.CurPlayer.Audio.PlayOneShot(GameManager.CurPlayer.playerCollectItemSound);

            if (dropItem != null)
                InventoryManager.Instance.AddItem(dropItem);

            // Return to pool; falls back to Destroy if not a pooled object
            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
