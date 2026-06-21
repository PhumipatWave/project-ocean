using Game.Core;
using UnityEngine;

namespace Game.Testing
{
    public class EnemyLoot : MonoBehaviour
    {
        [SerializeField] private Item dropItem;

        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        private Enemy enemy;
        private bool playerInRange;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            if (inputReader != null)
                inputReader.OnInteracted += HandleInteract;
        }

        private void OnDisable()
        {
            if (inputReader != null)
                inputReader.OnInteracted -= HandleInteract;
            playerInRange = false;
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed || !playerInRange || enemy == null || !enemy.IsDead) return;
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
            if (dropItem != null)
                InventoryManager.Instance?.AddItem(dropItem);

            if (GameManager.CurPlayer?.Audio != null)
                GameManager.CurPlayer.Audio.PlayOneShot(GameManager.CurPlayer.playerCollectItemSound);

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
