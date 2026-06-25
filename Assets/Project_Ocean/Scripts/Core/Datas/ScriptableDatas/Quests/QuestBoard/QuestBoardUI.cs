using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class QuestBoardUI : MonoBehaviour
    {
        [Header("Slots Management")]
        [SerializeField] private QuestSlot[] questSlots; // ลากวัตถุที่มี Script QuestSlot ทั้ง 4 ปุ่มมาใส่ช่องนี้

        [Header("Navigation Buttons")]
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button closeBoardButton;

        [Header("Sub Window Component")]
        [SerializeField] private QuestDetailPanel questDetailPanel; // ลากวัตถุหน้าต่างรายละเอียดมาใส่ช่องนี้

        private List<Quest> activeNewQuests = new List<Quest>();
        private int currentPageIndex = 0;
        private const int MaxQuestsPerPage = 4; // แสดงผลหน้าละ 4 เควสคงที่ตามรูป

        private void Start()
        {
            nextPageButton.onClick.AddListener(NavigateToNextPage);
            prevPageButton.onClick.AddListener(NavigateToPreviousPage);
            closeBoardButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnEnable()
        {
            currentPageIndex = 0;

            if (questDetailPanel != null)
                questDetailPanel.HidePanel();

            if (QuestManager.Instance != null)
            {
                QuestManager.OnQuestUpdated += FetchAndRefreshBoard;
                FetchAndRefreshBoard();
            }
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
                QuestManager.OnQuestUpdated -= FetchAndRefreshBoard;
        }

        public void FetchAndRefreshBoard()
        {
            if (QuestManager.Instance == null)
            {
                Debug.LogWarning("QuestManager ยังไม่พร้อม"); 
                return;
            }

            // คัดกรองเอาเฉพาะเควสที่มีสถานะเป็น New จากคลังเดิมของคุณมาโชว์บนบอร์ด
            activeNewQuests = QuestManager.Instance.GetQuestsByStatus(QuestStatus.New);
            RenderBoardPage();
        }

        private void RenderBoardPage()
        {
            int startingIndex = currentPageIndex * MaxQuestsPerPage;

            for (int i = 0; i < questSlots.Length; i++)
            {
                int targetQuestIndex = startingIndex + i;

                if (targetQuestIndex < activeNewQuests.Count)
                {
                    // ส่งข้อมูลเควสไปให้ Slot แสดงผลตัวเอง
                    questSlots[i].SetupSlot(activeNewQuests[targetQuestIndex], OnQuestSlotSelected);
                }
                else
                {
                    questSlots[i].ClearSlot();
                }
            }

            UpdateNavigationButtons(startingIndex);
        }

        private void UpdateNavigationButtons(int startingIndex)
        {
            prevPageButton.interactable = (currentPageIndex > 0);
            nextPageButton.interactable = (startingIndex + MaxQuestsPerPage < activeNewQuests.Count);
        }

        private void OnQuestSlotSelected(Quest selectedQuest)
        {
            if (questDetailPanel != null)
            {
                // สั่งให้หน้าต่างรายละเอียดแสดงผล และรับ Callback กลับมา Refresh บอร์ดเมื่อกดรับ/ปฏิเสธสำเร็จ
                questDetailPanel.ShowPanel(selectedQuest, OnQuestActionCompleted);
            }
        }

        private void OnQuestActionCompleted()
        {
            // เควสที่ถูกรับหรือปฏิเสธไปแล้วจะหายไปจากกระดานทันที จึงต้องคำนวณหน้ากระดานใหม่
            FetchAndRefreshBoard();
        }

        public void NavigateToNextPage()
        {
            if ((currentPageIndex + 1) * MaxQuestsPerPage < activeNewQuests.Count)
            {
                currentPageIndex++;
                RenderBoardPage();
            }
        }

        public void NavigateToPreviousPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                RenderBoardPage();
            }
        }
    }
}