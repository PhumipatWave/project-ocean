using Game.Core;
using UnityEngine;

namespace Game.Testing
{
    public class ItemDrop : CollectableItemBase
    {
        [Header("Drop Item")]
        [SerializeField] private Item dropItem;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip dropSound;

        public override void TryCollect()
        {
            if (InventoryManager.Instance == null || dropItem == null) return;
            if (audioSource != null && dropSound != null)
                audioSource.PlayOneShot(dropSound);
            InventoryManager.Instance.AddItem(dropItem);
            Destroy(transform.root.gameObject);
        }
    }
}
