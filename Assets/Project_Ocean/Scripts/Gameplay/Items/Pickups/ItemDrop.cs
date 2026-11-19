using System.Collections;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class ItemDrop : CollectableItemBase, IPoolable
    {
        [Header("Drop Item")]
        [SerializeField] private Item dropItem;


        [Tooltip("Seconds to wait before returning to pool after collection. 0 = immediate. Use 60 for shells.")]
        [SerializeField, Min(0f)] private float poolReturnDelay = 0f;

        /// Fired when the item is successfully collected. Subscribe to trigger post-collect animations.
        public event System.Action OnCollected;

        // Set at runtime by ItemSpawner. Overrides dropItem when assigned.
        private Item        _runtimeItem;
        private ItemSpawner _owner;

        private Item ActiveItem => _runtimeItem != null ? _runtimeItem : dropItem;

        /// Called by ItemSpawner after pulling this object from the pool.
        public void Setup(Item item, ItemSpawner owner)
        {
            _runtimeItem = item;
            _owner       = owner;
        }

        public override void TryCollect()
        {
            if (!IsCollectable) return;
            if (Inventory.Current == null || ActiveItem == null) return;

            bool added = Inventory.Current.AddItem(ActiveItem);
            if (!added) return; // inventory full — leave item in world

            AudioManager.Instance?.PlaySFX(SoundKeys.Collect);

            var root = transform.root.gameObject;
            _owner?.OnItemCollected(root);
            OnCollected?.Invoke();

            if (poolReturnDelay > 0f)
                StartCoroutine(DelayedReturn(poolReturnDelay));
            else if (_owner != null)
                ObjectPoolManager.Instance?.Return(root);
            else
                Destroy(root);
        }

        private IEnumerator DelayedReturn(float delay)
        {
            yield return new WaitForSeconds(delay);
            var root = transform.root.gameObject;
            if (_owner != null)
                ObjectPoolManager.Instance?.Return(root);
            else
                Destroy(root);
        }

        // ── IPoolable ──

        public void OnGetFromPool() { }

        public void OnReturnToPool()
        {
            StopAllCoroutines();
            _runtimeItem  = null;
            _owner        = null;
            IsCollectable = true;
        }
    }
}
