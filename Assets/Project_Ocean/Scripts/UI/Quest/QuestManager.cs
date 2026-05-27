using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Testing; // 💡 เพิ่มบรรทัดนี้เพื่อให้รู้จักระบบ Inventory และ Item ของคุณ

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Quest Data")]
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

    public void OpenQuestSystem(FishingNPC npc)
    {
        curNpc = npc;
        curQuest = npc.GetNpcQuest();

        if (curQuest == null) return;

        questWindow.SetActive(true);

        if (curQuest.Status == QuestStatus.New)
        {
            curStep = 0;
            TalkStep();
        }
        else if (curQuest.Status == QuestStatus.InProgress)
        {
            textNpcName.text = npc.NpcName;
            avatarNpc.sprite = npc.NpcAvatar;
            textPlayerName.text = "";
            avatarPlayer.sprite = null;

            textTalk.text = curQuest.QuestionInProgress;

            // 💡 เปลี่ยนมาเช็กไอเทมผ่านตัวแปร string itemID ใน InventoryManager.Instance ของคุณ
            if (InventoryManager.Instance.HasQuestItem(curQuest.QuestItemId))
            {
                buttonNext.SetActive(true);
                buttonClose.SetActive(false);
            }
            else
            {
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
            if (curStep == 0)
            {
                textNpcName.text = curNpc.NpcName;
                avatarNpc.sprite = curNpc.NpcAvatar;
                textTalk.text = curQuest.QuestDialogue[curStep];

                buttonNext.SetActive(true);
                buttonAccept.SetActive(false);
                buttonReject.SetActive(false);
            }
            else if (curStep == 1)
            {
                textPlayerName.text = "Player";
                textTalk.text = curQuest.AnswerNext[curStep - 1];

                buttonNext.SetActive(false);
                buttonAccept.SetActive(true);
                buttonReject.SetActive(true);
            }
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

            // 💡 สั่งหักไอเทมเควสและรับรางวัลผ่าน ID ชนิดข้อความ (string)
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
        questWindow.SetActive(false);
        curStep = 0;
    }
}