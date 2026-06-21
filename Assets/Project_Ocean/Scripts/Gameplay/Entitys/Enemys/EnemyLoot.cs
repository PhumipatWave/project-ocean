using Game.Core;
using UnityEngine;

namespace Game.Testing
{
    public class EnemyLoot : MonoBehaviour
    {
        [SerializeField] private Item dropItem;

        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        [SerializeField] private Enemy enemy;
        [SerializeField] private bool playerInRange;
        private bool isCollected;

        private void Awake()
        {
            enemy = GetComponentInParent<Enemy>();
        }

        private void OnEnable()
        {
            isCollected = false;
            if (inputReader != null)
                inputReader.OnInteracted += HandleInteract;
        }

        private void OnDisable()
        {
            if (inputReader != null)
                inputReader.OnInteracted -= HandleInteract;
        }

        private void HandleInteract(bool pressed)
        {
            if (isCollected || !playerInRange || enemy == null || !enemy.IsDead)
                return;

            Collect();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerInRange = false;
        }

        private void Collect()
        {
            isCollected = true;

            if (dropItem != null)
                InventoryManager.Instance?.AddItem(dropItem);

            if (GameManager.CurPlayer?.Audio != null)
                GameManager.CurPlayer.Audio.PlayOneShot(GameManager.CurPlayer.playerCollectItemSound);

            var root = enemy != null ? enemy.gameObject : gameObject;
            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(root);
            else
                Destroy(root);
        }
    }
}
