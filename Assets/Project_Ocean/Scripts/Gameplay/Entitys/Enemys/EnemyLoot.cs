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

            Destroy(gameObject);
        }
    }
}
