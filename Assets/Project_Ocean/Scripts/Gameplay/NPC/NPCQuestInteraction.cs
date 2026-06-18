using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Testing
{
    public class NPCQuestInteraction : MonoBehaviour
    {
        [Header("=== Quest Data Settings ===")]
        [SerializeField] private int targetQuestId;

        [Header("=== Dialogue UI Elements ===")]
        [SerializeField] private GameObject dialoguePanel;      // กล่องส้มด้านล่าง
        [SerializeField] private TextMeshProUGUI npcNameText;   // Text ชื่อ NPC
        [SerializeField] private TextMeshProUGUI dialogueText;  // Text คำพูด
        [SerializeField] private Image npcAvatarImage;          // คอมโพเนนต์ Image บน UI

        [Header("=== Quest UI Reference ===")]
        [SerializeField] private QuestDetailPanel questDetailPanel; // หน้าต่างรายละเอียดเควส

        private bool isPlayerNearby = false;
        private Quest npcQuest;
        private int currentDialogueIndex = 0;
        private bool isConversing = false;

        private void Start()
        {
            if (QuestManager.Instance != null)
            {
                npcQuest = QuestManager.Instance.GetQuestById(targetQuestId);
            }

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        private void Update()
        {
            if (!isPlayerNearby) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isConversing) StartDialogue();
                else AdvanceDialogue();
            }
        }

        private void StartDialogue()
        {
            if (npcQuest == null) return;

            isConversing = true;
            currentDialogueIndex = 0;

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            PlayerUIState.Open();

            // ดึงข้อมูลชื่อและรูปภาพจากข้อมูลเควสมาพ่นใส่หน้าจออัตโนมัติ
            if (npcNameText != null) npcNameText.text = npcQuest.NpcName;
            if (npcAvatarImage != null && npcQuest.NpcAvatar != null) npcAvatarImage.sprite = npcQuest.NpcAvatar;

            DetermineDialogueText();
        }

        private void DetermineDialogueText()
        {
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
                    dialogueText.text = "เปลี่ยนใจมาช่วยฉันหรือยังจ๊ะ?";
                    break;
            }
        }

        private void AdvanceDialogue()
        {
            if (npcQuest.Status == QuestStatus.New)
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < npcQuest.QuestDialogue.Length)
                {
                    dialogueText.text = npcQuest.QuestDialogue[currentDialogueIndex];
                }
                else
                {
                    EndDialogue();
                    // คุยจบปุ๊บ สั่งเปิดหน้าต่างรายละเอียดเควสต่อทันที
                    if (questDetailPanel != null) questDetailPanel.ShowPanel(npcQuest, null);
                }
            }
            else
            {
                EndDialogue();
            }
        }

        private void EndDialogue()
        {
            if (!isConversing) return;
            isConversing = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            PlayerUIState.Close();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")) isPlayerNearby = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isPlayerNearby = false;
                EndDialogue();
                if (questDetailPanel != null) questDetailPanel.HidePanel();
            }
        }
    }
}