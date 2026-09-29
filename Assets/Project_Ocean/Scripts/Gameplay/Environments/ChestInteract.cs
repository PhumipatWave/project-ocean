using Game.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Gameplay
{
    // Raw-key handling (E press) is delegated to an InteractUI on the same GameObject, wired
    // to call Interact() — same shared PlayerUIState "floor" lock every other interactable
    // (NPC/door/collectible) uses, instead of a second, ungated subscription that used to
    // open/close this chest's dialogue even while another panel was already open.
    [RequireComponent(typeof(InteractUI))]
    public class ChestInteract : MonoBehaviour
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
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI contentDisplay;
        [SerializeField] private GameObject portraitDisplay;
        [SerializeField] private GameObject extraImageObject;

        private static readonly List<string> globalOpenedChests = new();
        private string uniqueID;
        private bool wasOpened;
        private PlayerMoney targetPlayerMoney;
        private Image portraitImage;
        private Image extraImage;
        private InteractUI interactUI;

        void Awake()
        {
            uniqueID = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_" + transform.position;
            wasOpened = globalOpenedChests.Contains(uniqueID);

            if (portraitDisplay != null) portraitImage = portraitDisplay.GetComponent<Image>();
            if (extraImageObject != null) extraImage = extraImageObject.GetComponent<Image>();
            interactUI = GetComponent<InteractUI>();

            // The scene's InteractUI had no onInteract call wired, so E did nothing.
            if (interactUI.onInteract.GetPersistentEventCount() == 0)
                interactUI.onInteract.AddListener(Interact);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                targetPlayerMoney = other.GetComponent<PlayerMoney>();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                targetPlayerMoney = null;
                CloseUI();
            }
        }

        /// Called by InteractUI's onInteract (wired in the Inspector) on E press.
        public void Interact()
        {
            if (!dialogueBox.activeSelf) OpenUI();
            else CloseUI();
        }

        /// Wire the on-screen interact button's onClick here — same path as pressing E.
        public void PressInteract() => PlayerRegistry.Local?.input?.TriggerInteract();

        void OpenUI()
        {
            if (dialogueBox == null) return;

            string currentText;
            Sprite currentPortrait;
            Sprite currentExtraSprite;

            if (!wasOpened)
            {
                wasOpened = true;
                if (!globalOpenedChests.Contains(uniqueID))
                    globalOpenedChests.Add(uniqueID);

                targetPlayerMoney?.AddMoney(moneyAmount);
                currentText       = string.Format(firstDialogue, moneyAmount);
                currentPortrait   = firstPortrait;
                currentExtraSprite = firstExtraSprite;
            }
            else
            {
                currentText       = secondDialogue;
                currentPortrait   = secondPortrait;
                currentExtraSprite = secondExtraSprite;
            }

            if (contentDisplay != null) contentDisplay.text = currentText;

            if (portraitDisplay != null)
            {
                portraitDisplay.SetActive(currentPortrait != null);
                if (portraitImage != null && currentPortrait != null) portraitImage.sprite = currentPortrait;
            }

            if (extraImageObject != null)
            {
                extraImageObject.SetActive(currentExtraSprite != null);
                if (extraImage != null && currentExtraSprite != null) extraImage.sprite = currentExtraSprite;
            }

            dialogueBox.SetActive(true);
            Time.timeScale = 0f;
        }

        void CloseUI()
        {
            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (extraImageObject != null) extraImageObject.SetActive(false);
            Time.timeScale = 1f;
            interactUI?.ReleasePlayerLock();
        }
    }
}
