using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Testing;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Quest Database")]
    // คลังเก็บเควสทั้งหมดในเกม (สร้างไฟล์ QuestData แล้วมาลากใส่ตรงนี้ให้ครบ)
    [SerializeField] private QuestData[] allQuestData;

    [Header("UI Panels")]
    [SerializeField] private GameObject questWindow;
    [SerializeField] private TextMeshProUGUI textTalk;
    [SerializeField] private TextMeshProUGUI textNpcName;
    [SerializeField] private TextMeshProUGUI textPlayerName;
    [SerializeField] private Image avatarNpc;
    [SerializeField] private Image avatarPlayer;

    [Header("UI Buttons")]
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

    // 💡 ฟังก์ชันใหม่: ค้นหาเควสในคลัง All Quest Data ด้วย ID
    public Quest GetQuestById(int id)
    {
        if (allQuestData == null) return null;

        foreach (QuestData data in allQuestData)
        {
            if (data != null && data.questId == id)
            {
                // ส่งเควสในรูปแบบ Runtime กลับไปใช้งาน
                return new Quest(data);
            }
        }
        Debug.LogError($"ไม่พบเควส ID: {id} ใน All Quest Data ของ QuestManager!");
        return null;
    }

    // ปรับให้รับค่า FishingNPC เข้ามาเหมือนเดิม แต่ข้างในจะไปดึงเควสจากคลังส่วนกลางแทน
    public void OpenQuestSystem(FishingNPC npc)
    {
        curNpc = npc;

        // 💡 เปลี่ยนตรงนี้: ให้วิ่งไปถามคลังส่วนกลางตามรหัส ID ที่ NPC คนนี้พกมา
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
        textPlayerName.text = "Player";
        textTalk.text = curQuest.AnswerAccept;

        buttonAccept.SetActive(false);
        buttonReject.SetActive(false);
        buttonClose.SetActive(true);
    }

    public void RejectQuest()
    {
        curQuest.Status = QuestStatus.Reject;
        textPlayerName.text = "Player";
        textTalk.text = curQuest.AnswerReject;

        buttonAccept.SetActive(false);
        buttonReject.SetActive(false);
        buttonClose.SetActive(true);
    }

    public void CloseQuestWindow()
    {
        questWindow.SetActive(false);
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