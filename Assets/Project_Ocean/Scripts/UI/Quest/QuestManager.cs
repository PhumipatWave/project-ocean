using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // 💡 เพิ่มบรรทัดนี้เพื่อใช้งาน List
using Game.Testing;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Quest Database")]
    [SerializeField] private QuestData[] allQuestData;

    // 💡 คลังจำสถานะเควสในแรม ป้องกันสถานะเควสรีเซ็ตกลับเป็นค่าเริ่มต้น
    private List<Quest> runtimeQuestList = new List<Quest>();

    [Header("Quest Board List Slots")]
    [SerializeField] private GameObject questBoardPanel;
    [SerializeField] private GameObject questDetailPanel;
    [SerializeField] private Button[] boardQuestButtons;
    [SerializeField] private TextMeshProUGUI[] boardQuestTitleTexts;

    [Header("Quest Detail Displays")]
    [SerializeField] private TextMeshProUGUI nameDatailText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;

    // 💡 แยกปุ่มควบคุมของหน้าบอร์ดออกมาให้ชัดเจน ไม่ให้ปนกับหน้า NPC
    [SerializeField] private GameObject boardAcceptButton;
    [SerializeField] private GameObject boardCloseButton;

    private int[] currentBoardQuestIds;
    private Quest selectedBoardQuest;

    [Header("UI Panels (General NPC)")]
    [SerializeField] private GameObject questWindow;
    [SerializeField] private TextMeshProUGUI textTalk;
    [SerializeField] private TextMeshProUGUI textNpcName;
    [SerializeField] private TextMeshProUGUI textPlayerName;
    [SerializeField] private Image avatarNpc;
    [SerializeField] private Image avatarPlayer;

    [Header("UI Buttons (General NPC)")]
    [SerializeField] private GameObject buttonNext;
    [SerializeField] private GameObject buttonAccept;
    [SerializeField] private GameObject buttonReject;
    [SerializeField] private GameObject buttonClose;

    private Quest curQuest;
    private int curStep = 0;
    private FishingNPC curNpc;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 💡 บังคับแปลงไฟล์ Asset ทั้งหมดมาเก็บใน List ส่วนกลางตั้งแต่เริ่มเกม เพื่อให้เก็บสถานะได้ถาวร
        runtimeQuestList = new List<Quest>();
        if (allQuestData != null)
        {
            foreach (QuestData data in allQuestData)
            {
                if (data != null) runtimeQuestList.Add(new Quest(data));
            }
        }
    }

    // 💡 ปรับปรุงใหม่: ค้นหาจากคลังแรมส่วนกลาง (ตัวเดียวกับที่บันทึกสถานะได้)
    public Quest GetQuestById(int id)
    {
        foreach (Quest q in runtimeQuestList)
        {
            if (q != null && q.QuestID == id) return q;
        }
        return null;
    }

    // ================= ระบบป้ายประกาศ (Quest Board) =================

    public void OpenQuestBoard(int[] questIds)
    {
        currentBoardQuestIds = questIds;
        Time.timeScale = 0f;

        if (questBoardPanel != null) questBoardPanel.SetActive(true);
        if (questDetailPanel != null) questDetailPanel.SetActive(false);

        for (int i = 0; i < boardQuestButtons.Length; i++)
        {
            if (i < questIds.Length)
            {
                Quest questData = GetQuestById(questIds[i]);

                // ตรวจเช็กจากคลังแรมจริงๆ ถ้ายังเป็นเควสใหม่ (New) ให้เปิดปุ่มโชว์บนบอร์ด
                if (questData != null && questData.Status == QuestStatus.New)
                {
                    boardQuestButtons[i].gameObject.SetActive(true);
                    boardQuestTitleTexts[i].text = questData.QuestName;

                    int index = i;
                    boardQuestButtons[i].onClick.RemoveAllListeners();
                    boardQuestButtons[i].onClick.AddListener(() => ClickSelectQuestOnBoard(questIds[index]));
                }
                else
                {
                    boardQuestButtons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                boardQuestButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ClickSelectQuestOnBoard(int questId)
    {
        selectedBoardQuest = GetQuestById(questId);
        if (selectedBoardQuest == null) return;

        if (questBoardPanel != null) questBoardPanel.SetActive(false);
        if (questDetailPanel != null) questDetailPanel.SetActive(true);

        if (nameDatailText != null) nameDatailText.text = selectedBoardQuest.QuestName;
        if (detailDescriptionText != null) detailDescriptionText.text = selectedBoardQuest.QuestDetail;

        // เปิดปุ่มเฉพาะของหน้าบอร์ดแผ่นกระดาษรายละเอียด
        if (boardAcceptButton != null) boardAcceptButton.SetActive(true);
        if (boardCloseButton != null) boardCloseButton.SetActive(true);
    }

    public void AcceptBoardQuest()
    {
        if (selectedBoardQuest != null)
        {
            selectedBoardQuest.Status = QuestStatus.InProgress;
            Debug.Log($"[Quest Board] รับเควสสำเร็จ! ID {selectedBoardQuest.QuestID} สถานะตอนนี้คือ InProgress");
            CloseQuestWindow();
        }
    }

    // ================= ระบบคุยกับ NPC ปกติ =================
    public void OpenQuestSystem(FishingNPC npc)
    {
        curNpc = npc;
        curQuest = GetQuestById(npc.AssignQuestId);

        if (curQuest == null) return;

        Time.timeScale = 0f;
        questWindow.SetActive(true);

        if (curQuest.Status == QuestStatus.New)
        {
            curStep = 0;
            TalkStep();
        }
        else if (curQuest.Status == QuestStatus.InProgress)
        {
            SetupNpcUI();
            textTalk.text = curQuest.QuestionInProgress;

            if (InventoryManager.Instance.HasQuestItem(curQuest.QuestItemId))
            {
                buttonNext.SetActive(true);
                buttonClose.SetActive(false);
            }
            else
            {
                textPlayerName.text = "Player";
                textTalk.text = curQuest.AnswerNotFinish;
                buttonNext.SetActive(false);
                buttonClose.SetActive(true);
            }
            buttonAccept.SetActive(false);
            buttonReject.SetActive(false);
        }
    }

    public void TalkStep()
    {
        if (curQuest.Status == QuestStatus.New)
        {
            if (curQuest.QuestDialogue == null || curStep >= curQuest.QuestDialogue.Length)
            {
                SetupPlayerUI();
                textTalk.text = "คุณจะรับคำขอนี้ไหม?";
                buttonNext.SetActive(false);
                buttonAccept.SetActive(true);
                buttonReject.SetActive(true);
                return;
            }

            SetupNpcUI();
            textTalk.text = curQuest.QuestDialogue[curStep];

            buttonNext.SetActive(true);
            buttonAccept.SetActive(false);
            buttonReject.SetActive(false);
            buttonClose.SetActive(false);
        }
    }

    public void ClickNextButton()
    {
        if (curQuest.Status == QuestStatus.New)
        {
            curStep++;
            TalkStep();
        }
        else if (curQuest.Status == QuestStatus.InProgress)
        {
            textPlayerName.text = "Player";
            textTalk.text = curQuest.AnswerFinish;

            InventoryManager.Instance.RemoveQuestItem(curQuest.QuestItemId);
            InventoryManager.Instance.AddRewardItem(curQuest.RewardItemId);

            curQuest.Status = QuestStatus.Finish;

            buttonNext.SetActive(false);
            buttonClose.SetActive(true);
        }
    }

    public void AcceptQuest()
    {
        curQuest.Status = QuestStatus.InProgress;
        textTalk.text = curQuest.AnswerAccept;
        buttonAccept.SetActive(false);
        buttonReject.SetActive(false);
        buttonClose.SetActive(true);
    }

    public void RejectQuest()
    {
        curQuest.Status = QuestStatus.Reject;
        textTalk.text = curQuest.AnswerReject;
        buttonAccept.SetActive(false);
        buttonReject.SetActive(false);
        buttonClose.SetActive(true);
    }

    public void CloseQuestWindow()
    {
        if (questWindow != null) questWindow.SetActive(false);
        if (questBoardPanel != null) questBoardPanel.SetActive(false);
        if (questDetailPanel != null) questDetailPanel.SetActive(false);

        curStep = 0;
        Time.timeScale = 1f;
    }

    private void SetupNpcUI()
    {
        textNpcName.text = curNpc.NpcName;
        if (avatarNpc != null)
        {
            avatarNpc.gameObject.SetActive(true);
            avatarNpc.sprite = curNpc.NpcAvatar;
        }
        textPlayerName.text = "";
        if (avatarPlayer != null) avatarPlayer.gameObject.SetActive(false);
    }

    private void SetupPlayerUI()
    {
        textNpcName.text = "";
        if (avatarNpc != null) avatarNpc.gameObject.SetActive(false);
        textPlayerName.text = "Player";
        if (avatarPlayer != null) avatarPlayer.gameObject.SetActive(true);
    }
}