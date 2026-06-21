using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Testing
{
    public class NPCInteract : MonoBehaviour
    {
        [Header("NPC Settings")]
        [SerializeField] private bool hasQuest;

        [Header("Normal Dialogue")]
        [TextArea(3, 10)]
        [SerializeField] private string npcDialogue = "...";
        [SerializeField] private Sprite npcPortrait;

        [Header("Quest Settings")]
        [SerializeField] private int targetQuestId;
        [SerializeField] private QuestDetailPanel questDetailPanel;

        [Header("Dialogue UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image npcAvatarImage;

        private Quest npcQuest;
        private int currentDialogueIndex;
        private bool isConversing;
        private bool isUIOpen;
        private InteractUI interactUI;

        private void Start()
        {
            interactUI = GetComponent<InteractUI>();

            if (hasQuest && QuestManager.Instance != null)
                npcQuest = QuestManager.Instance.GetQuestById(targetQuestId);

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        public void Interact()
        {
            if (hasQuest) 
                QuestInteract();
            else 
                ToggleDialogue();
        }

        private void ToggleDialogue()
        {
            if (dialoguePanel != null && !dialoguePanel.activeSelf) 
                OpenDialogue();
            else 
                CloseDialogue();
        }

        private void OpenDialogue()
        {
            if (dialoguePanel == null) 
                return;

            if (dialogueText != null)
                dialogueText.text = npcDialogue;

            if (npcAvatarImage != null)
            {
                npcAvatarImage.gameObject.SetActive(npcPortrait != null);
                if (npcPortrait != null) 
                    npcAvatarImage.sprite = npcPortrait;
            }
            dialoguePanel.SetActive(true);
            isUIOpen = true;
        }

        private void QuestInteract()
        {
            if (!isConversing)
                StartDialogue();
            else
                AdvanceDialogue();
        }

        private void StartDialogue()
        {
            if (npcQuest == null) 
                return;

            isConversing = true;
            currentDialogueIndex = 0;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            isUIOpen = true;

            if (npcNameText != null)
                npcNameText.text = npcQuest.NpcName;

            if (npcAvatarImage != null && npcQuest.NpcAvatar != null) 
                npcAvatarImage.sprite = npcQuest.NpcAvatar;

            ShowDialogueText();
        }

        private void ShowDialogueText()
        {
            if (dialogueText == null) 
                return;

            switch (npcQuest.Status)
            {
                case QuestStatus.New:       
                    dialogueText.text = npcQuest.QuestDialogue[currentDialogueIndex]; 
                    break;
                case QuestStatus.InProgress:
                    dialogueText.text = npcQuest.QuestionInProgress;       
                    break;
                case QuestStatus.Finish:  
                    dialogueText.text = npcQuest.AnswerFinish;                  
                    break;
                case QuestStatus.Reject:   
                    dialogueText.text = "?";
                    break;
            }
        }

        private void AdvanceDialogue()
        {
            if (npcQuest.Status == QuestStatus.New)
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < npcQuest.QuestDialogue.Length)
                    dialogueText.text = npcQuest.QuestDialogue[currentDialogueIndex];
                else
                {
                    CloseDialogue();

                    if (questDetailPanel != null) 
                        questDetailPanel.ShowPanel(npcQuest, null);
                }
            }
            else
            {
                CloseDialogue();
            }
        }

        private void CloseDialogue()
        {
            isConversing = false;
            if (dialoguePanel != null) 
                dialoguePanel.SetActive(false);

            if (isUIOpen)
            {
                interactUI?.ReleasePlayerLock();
                isUIOpen = false;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) 
                return;

            CloseDialogue();

            if (hasQuest && questDetailPanel != null)
                questDetailPanel.HidePanel();
        }
    }
}
