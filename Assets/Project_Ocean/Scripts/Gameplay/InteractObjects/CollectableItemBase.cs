using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public abstract class CollectableItemBase : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        [Header("UI")]
        [SerializeField] private GameObject collectButton;

        protected bool playerInRange;

        /// External components (e.g. ShelleController) can set this to false to block collection.
        public bool IsCollectable { get; set; } = true;

        protected virtual bool CanCollect =>
            IsCollectable && FeatureUnlockService.Instance.IsUnlocked(GameFeature.InteractItem);

        private static readonly HashSet<CollectableItemBase> _itemsInRange = new();
        private static Transform _playerTransform;

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
            _itemsInRange.Remove(this);
            playerInRange = false;
        }

        protected virtual void Update()
        {
            if (collectButton == null) return;
            collectButton.SetActive(playerInRange && CanCollect);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = true;
                _playerTransform = collision.transform;
                _itemsInRange.Add(this);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = false;
                _itemsInRange.Remove(this);
            }
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed) return;
            if (PlayerUIState.IsLocked) return; // some other interactable already has the floor
            if (PlayerSelfDialogueUI.BlocksInteraction) return; // the press that closed an auto dialogue
            if (!playerInRange || !CanCollect) return;
            if (!IsClosestToPlayer()) return;
            TryCollect();
        }

        private bool IsClosestToPlayer()
        {
            if (_playerTransform == null || _itemsInRange.Count <= 1) return true;

            float myDist = Vector2.SqrMagnitude((Vector2)transform.position - (Vector2)_playerTransform.position);
            foreach (var other in _itemsInRange)
            {
                if (other == this || !other.CanCollect) continue;
                float otherDist = Vector2.SqrMagnitude((Vector2)other.transform.position - (Vector2)_playerTransform.position);
                if (otherDist < myDist) return false;
            }
            return true;
        }

        public abstract void TryCollect();
    }
}
