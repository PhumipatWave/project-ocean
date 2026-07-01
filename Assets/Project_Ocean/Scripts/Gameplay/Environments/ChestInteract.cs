using Game.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Testing
{
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
        private bool _subscribed;
        private PlayerMoney targetPlayerMoney;
        private Image portraitImage;
        private Image extraImage;

        void Awake()
        {
            uniqueID = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_" + transform.position;
            wasOpened = globalOpenedChests.Contains(uniqueID);

            if (portraitDisplay != null) portraitImage = portraitDisplay.GetComponent<Image>();
            if (extraImageObject != null) extraImage = extraImageObject.GetComponent<Image>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                targetPlayerMoney = other.GetComponent<PlayerMoney>();
                Subscribe();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                targetPlayerMoney = null;
                Unsubscribe();
                CloseUI();
            }
        }

        private void OnDisable() => Unsubscribe();

        private void HandleInteract(bool pressed)
        {
            if (!pressed) return;
            if (!dialogueBox.activeSelf) OpenUI();
            else CloseUI();
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            var reader = GameManager.CurPlayer?.input;
            if (reader == null) return;
            reader.OnInteracted += HandleInteract;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted -= HandleInteract;
            _subscribed = false;
        }

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
        }
    }
}
