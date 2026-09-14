using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Testing
{
    /// <summary>Raised whenever the player interacts with an NPC. Payload = that NPC's npcId.</summary>
    public static class NpcEvents
    {
        public static event System.Action<string> OnNpcTalkedTo;

        public static void RaiseNpcTalkedTo(string npcId)
        {
            OnNpcTalkedTo?.Invoke(npcId);
        }
    }

    public class NPC : MonoBehaviour
    {
        [Header("NPC Settings")]
        [SerializeField] private bool hasQuest;

        [Tooltip("Stable id for this NPC, matched by Talk-type quest steps (Target NPC Id). Leave blank if no quest ever needs to target this specific NPC.")]
        [SerializeField] private string npcId;

        [Header("Normal Dialogue")]
        [SerializeField] private string npcName;
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

        public string NpcId => npcId;
        public string NpcName => npcName;
        public Sprite NpcPortrait => npcPortrait;

        private Quest npcQuest;
        private int currentDialogueIndex;
        private bool isConversing;
        private bool isUIOpen;
        private InteractUI interactUI;

        private void OnEnable() => NPCManager.Instance?.Register(this);
        private void OnDisable() => NPCManager.Instance?.Unregister(this);

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
            if (!string.IsNullOrEmpty(npcId))
                NpcEvents.RaiseNpcTalkedTo(npcId);

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

            // A New quest with an unfinished prerequisite isn't offered yet.
            if (npcQuest.Status == QuestStatus.New &&
                QuestManager.Instance != null && !QuestManager.Instance.IsUnlocked(npcQuest))
                return;

            isConversing = true;
            currentDialogueIndex = 0;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            isUIOpen = true;

            // Show THIS physical NPC's own identity, not the quest's — several NPCs can
            // share one targetQuestId (e.g. different Talk steps of the same quest), and
            // each should be labeled as themselves when the player talks to them.
            if (npcNameText != null)
                npcNameText.text = npcName;

            if (npcAvatarImage != null && npcPortrait != null)
                npcAvatarImage.sprite = npcPortrait;

            ShowDialogueText();
        }

        private void ShowDialogueText()
        {
            if (dialogueText == null)
                return;

            switch (npcQuest.Status)
            {
                case QuestStatus.New:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.Offer, currentDialogueIndex);
                    break;
                case QuestStatus.InProgress:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.InProgress);
                    break;
                // FIX 1: ReadyToTurnIn was missing — caused blank dialogue on Turn In attempt
                case QuestStatus.ReadyToTurnIn:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.Finish);
                    break;
                case QuestStatus.Finish:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.Finish);
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
                var offerLines = npcQuest.CurrentStep.GetLines(npcId, DialogueType.Offer);
                if (currentDialogueIndex < offerLines.Length)
                    dialogueText.text = offerLines[currentDialogueIndex];
                else
                {
                    CloseDialogue();
                    if (questDetailPanel != null)
                        questDetailPanel.ShowPanel(npcQuest, null);
                }
            }
            // FIX 2: ReadyToTurnIn fell into the else-CloseDialogue branch — quest never completed
            else if (npcQuest.Status == QuestStatus.ReadyToTurnIn)
            {
                CloseDialogue();
                bool success = QuestManager.Instance.TurnInQuest(npcQuest.QuestID);
                if (!success)
                    Debug.LogWarning($"[NPCInteract] TurnInQuest failed for id {npcQuest.QuestID}");
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
