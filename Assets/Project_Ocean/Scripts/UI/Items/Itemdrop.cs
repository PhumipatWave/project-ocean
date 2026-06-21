using Game.Core;
using UnityEngine;

namespace Game.Testing
{
    public class Itemdrop : MonoBehaviour
    {
        [Header("Drop Item")]
        [SerializeField] private Item dropItem;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip dropSound;

        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        private bool playerInRange;

        private void OnEnable()
        {
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
            if (!pressed || !playerInRange) return;
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
            if (InventoryManager.Instance == null || dropItem == null) return;
            if (audioSource != null && dropSound != null)
                audioSource.PlayOneShot(dropSound);
            InventoryManager.Instance.AddItem(dropItem);
            Destroy(gameObject);
        }
    }
}
