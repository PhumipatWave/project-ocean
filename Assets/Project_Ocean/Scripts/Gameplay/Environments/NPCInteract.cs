using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Testing
{
    public class NPCInteract : MonoBehaviour
    {
        [Header("NPC Settings")]
        [TextArea(3, 10)]
        [SerializeField] private string dialogueText = "...";
        [SerializeField] private Sprite npcPortrait;

        [Header("UI Reference")]
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI contentDisplay;
        [SerializeField] private GameObject portraitDisplay;

        private bool isPlayerInRange;
        private Image portraitImage;

        void Awake()
        {
            if (portraitDisplay != null) portraitImage = portraitDisplay.GetComponent<Image>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) isPlayerInRange = true;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                CloseDialogue();
            }
        }

        void Update()
        {
            if (isPlayerInRange && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                if (dialogueBox != null && !dialogueBox.activeSelf) OpenDialogue();
                else CloseDialogue();
            }
        }

        void OpenDialogue()
        {
            if (dialogueBox == null) return;

            if (contentDisplay != null) contentDisplay.text = dialogueText;

            if (portraitDisplay != null)
            {
                portraitDisplay.SetActive(npcPortrait != null);
                if (portraitImage != null && npcPortrait != null) portraitImage.sprite = npcPortrait;
            }

            dialogueBox.SetActive(true);
            Time.timeScale = 0f;
        }

        void CloseDialogue()
        {
            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (portraitDisplay != null) portraitDisplay.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
