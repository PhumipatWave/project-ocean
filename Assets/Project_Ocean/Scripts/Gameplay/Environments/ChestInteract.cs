using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        private bool isPlayerInRange;
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
                isPlayerInRange = true;
                targetPlayerMoney = other.GetComponent<PlayerMoney>();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                targetPlayerMoney = null;
                CloseUI();
            }
        }

        void Update()
        {
            if (isPlayerInRange && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                if (!dialogueBox.activeSelf) OpenUI();
                else CloseUI();
            }
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
