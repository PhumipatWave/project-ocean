using Game.Core;
using UnityEngine;

namespace Game.Testing
{
    public abstract class CollectableItemBase : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        [Header("UI")]
        [SerializeField] private GameObject collectButton;

        protected bool playerInRange;

        protected virtual bool CanCollect => true;

        protected virtual void OnEnable()
        {
            if (inputReader != null)
                inputReader.OnInteracted += HandleInteract;
            if (collectButton != null) collectButton.SetActive(false);
        }

        protected virtual void OnDisable()
        {
            if (inputReader != null)
                inputReader.OnInteracted -= HandleInteract;
        }

        protected virtual void Update()
        {
            if (collectButton == null) return;
            collectButton.SetActive(playerInRange && CanCollect);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerInRange = true;
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                playerInRange = false;
        }

        private void HandleInteract(bool pressed)
        {
            if (!playerInRange) return;
            TryCollect();
        }

        public abstract void TryCollect();
    }
}
