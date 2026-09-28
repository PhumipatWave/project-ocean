using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class EnemyLoot : CollectableItemBase
    {
        [SerializeField] private Item dropItem;
        [SerializeField] private Enemy enemy;

        private bool isCollected;

        protected override bool CanCollect => enemy != null && enemy.IsDead && !isCollected;

        private void Awake()
        {
            enemy = GetComponentInParent<Enemy>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            isCollected = false;
        }

        public override void TryCollect()
        {
            if (!CanCollect) return;
            Collect();
        }

        public void Collect()
        {
            isCollected = true;

            if (dropItem != null)
            {
                bool added = Inventory.Current?.AddItem(dropItem) ?? false;
                if (!added)
                {
                    isCollected = false;
                    return;
                }
            }

            if (GameManager.CurPlayer != null)
                AudioManager.Instance?.PlaySFX(SoundKeys.Collect);

            var root = enemy != null ? enemy.gameObject : gameObject;
            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(root);
            else
                Destroy(root);
        }
    }
}
