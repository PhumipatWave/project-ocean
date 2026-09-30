using Game.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    public class ChestInteract : Interactable
    {
        [Header("Settings")]
        [SerializeField] private int moneyAmount = 100;

        [Header("First Interaction")]
        [TextArea(3, 10)][SerializeField] private string firstDialogue;
        [SerializeField] private Sprite firstPortrait;
        [SerializeField] private Sprite firstExtraSprite;

        [Header("Subsequent Interaction")]
        [TextArea(3, 10)][SerializeField] private string secondDialogue;
        [SerializeField] private Sprite secondPortrait;
        [SerializeField] private Sprite secondExtraSprite;

        [Header("UI Reference")]
        [Tooltip("InteractPopupView on the chest's dialogue panel (an IInteractPopupView).")]
        [SerializeField] private MonoBehaviour popupView;

        // Not saved on purpose: opened chests refill after the game restarts.
        private static readonly HashSet<string> globalOpenedChests = new();
        private string uniqueID;

        private IInteractPopupView Popup => popupView as IInteractPopupView;

        void Awake()
        {
            uniqueID = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_" + transform.position;
        }

        protected override void Interact(Player player)
        {
            if (Popup == null) return;

            if (Popup.IsOpen) Close();
            else Open(player);
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) Close();
            base.OnTriggerExit2D(other);
        }

        void Open(Player player)
        {
            if (globalOpenedChests.Add(uniqueID))
            {
                player.GetComponent<PlayerMoney>()?.AddMoney(moneyAmount);
                Popup.Show(null, string.Format(firstDialogue, moneyAmount), firstPortrait, firstExtraSprite);
            }
            else
            {
                Popup.Show(null, secondDialogue, secondPortrait, secondExtraSprite);
            }

            Time.timeScale = 0f;
        }

        void Close()
        {
            Popup?.Hide();
            Time.timeScale = 1f;
            ReleasePlayerLock();
        }
    }
}
