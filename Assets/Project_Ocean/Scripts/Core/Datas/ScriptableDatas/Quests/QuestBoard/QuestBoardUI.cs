using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Testing
{
    public class QuestBoardUI : MonoBehaviour
    {
        [Header("Quest Board Title")]
        [SerializeField] private TextMeshProUGUI questBoardTitle;

        [Header("Category Page")]
        [SerializeField] private GameObject categoryPage;
        [SerializeField] private QuestCategorySlot[] categorySlots;

        [Header("Quest List Page")]
        [SerializeField] private GameObject questListPage;
        [SerializeField] private QuestSlot[] questSlots;

        [Header("Quest List Title")]
        [SerializeField] private TextMeshProUGUI questCategoryTitle;

        [Header("Navigation")]
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button backToCategoryButton;
        [SerializeField] private Button closeBoardButton;

        [Header("Quest Detail")]
        [SerializeField] private QuestDetailPanel questDetailPanel;

        [Header("Category Settings")]
        [SerializeField] private QuestCategoryUIData[] categories;

        private List<Quest> questsInCurrentCategory =
            new List<Quest>();

        private QuestCategory currentCategory;

        private int currentPageIndex = 0;

        private const int MaxQuestsPerPage = 4;

        [System.Serializable]
        public class QuestCategoryUIData
        {
            public QuestCategory category;
        }

        private void Start()
        {
            if (nextPageButton != null)
                nextPageButton.onClick.AddListener(
                    NavigateToNextPage
                );

            if (prevPageButton != null)
                prevPageButton.onClick.AddListener(
                    NavigateToPreviousPage
                );

            if (backToCategoryButton != null)
                backToCategoryButton.onClick.AddListener(
                    ShowCategoryPage
                );

            if (closeBoardButton != null)
                closeBoardButton.onClick.AddListener(
                    () => gameObject.SetActive(false)
                );
        }

        private void OnEnable()
        {
            currentPageIndex = 0;

            if (QuestManager.Instance != null)
            {
                QuestManager.OnQuestUpdated += RefreshCurrentPage;
            }

            ShowCategoryPage();
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.OnQuestUpdated -= RefreshCurrentPage;
            }
        }

        // =========================================================
        // CATEGORY PAGE
        // =========================================================

        private void ShowCategoryPage()
        {
            currentPageIndex = 0;

            // เปลี่ยนชื่อกลับเป็น Quest Board
            if (questBoardTitle != null)
            {
                questBoardTitle.text = "Quest Board";
            }

            if (questDetailPanel != null)
                questDetailPanel.HidePanel();

            if (categoryPage != null)
                categoryPage.SetActive(true);

            if (questListPage != null)
                questListPage.SetActive(false);

            RenderCategories();
        }

        private void RenderCategories()
        {
            for (int i = 0; i < categorySlots.Length; i++)
            {
                if (i < categories.Length)
                {
                    QuestCategoryUIData data = categories[i];

                    categorySlots[i].Setup(
                        data.category,
                        OnCategorySelected
                    );
                }
                else
                {
                    categorySlots[i].Clear();
                }
            }
        }

        private void OnCategorySelected(QuestCategory category)
        {
            currentCategory = category;
            currentPageIndex = 0;

            // เปลี่ยนชื่อหัวกระดาน
            if (questBoardTitle != null)
            {
                switch (category)
                {
                    case QuestCategory.FishS:
                        questBoardTitle.text = "Quest Fish S";
                        break;

                    case QuestCategory.FishM:
                        questBoardTitle.text = "Quest Fish M";
                        break;

                    case QuestCategory.FishL:
                        questBoardTitle.text = "Quest Fish L";
                        break;

                    default:
                        questBoardTitle.text = "Quest Board";
                        break;
                }
            }

            LoadQuestsForCategory();

            if (categoryPage != null)
                categoryPage.SetActive(false);

            if (questListPage != null)
                questListPage.SetActive(true);

            RenderQuestPage();
        }

        // =========================================================
        // QUEST LIST PAGE
        // =========================================================

        private void LoadQuestsForCategory()
        {
            questsInCurrentCategory.Clear();

            if (QuestManager.Instance == null)
            {
                Debug.LogError("[QuestBoardUI] QuestManager.Instance is NULL!");
                return;
            }

            questsInCurrentCategory =
                QuestManager.Instance.GetQuestsByCategory(currentCategory);

            Debug.Log(
                $"[QuestBoardUI] Category = {currentCategory}, " +
                $"Quest Count = {questsInCurrentCategory.Count}"
            );
        }

        private void RenderQuestPage()
        {
            int startingIndex =
                currentPageIndex * MaxQuestsPerPage;

            for (int i = 0; i < questSlots.Length; i++)
            {
                int targetIndex = startingIndex + i;

                if (targetIndex < questsInCurrentCategory.Count)
                {
                    questSlots[i].SetupSlot(
                        questsInCurrentCategory[targetIndex],
                        OnQuestSlotSelected
                    );
                }
                else
                {
                    questSlots[i].ClearSlot();
                }
            }

            if (questCategoryTitle != null)
            {
                questCategoryTitle.text =
                    currentCategory.ToString();
            }

            UpdateNavigationButtons(startingIndex);
        }

        private void UpdateNavigationButtons(
            int startingIndex)
        {
            if (prevPageButton != null)
            {
                prevPageButton.interactable =
                    currentPageIndex > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable =
                    startingIndex + MaxQuestsPerPage
                    < questsInCurrentCategory.Count;
            }
        }

        // =========================================================
        // QUEST DETAIL
        // =========================================================

        private void OnQuestSlotSelected(
            Quest selectedQuest)
        {
            if (questDetailPanel == null)
                return;

            questDetailPanel.ShowPanel(
                selectedQuest,
                OnQuestActionCompleted
            );
        }

        private void OnQuestActionCompleted()
        {
            LoadQuestsForCategory();
            RenderQuestPage();
        }

        // =========================================================
        // NAVIGATION
        // =========================================================

        public void NavigateToNextPage()
        {
            if ((currentPageIndex + 1) * MaxQuestsPerPage
                < questsInCurrentCategory.Count)
            {
                currentPageIndex++;
                RenderQuestPage();
            }
        }

        public void NavigateToPreviousPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                RenderQuestPage();
            }
        }

        // =========================================================
        // REFRESH
        // =========================================================

        private void RefreshCurrentPage()
        {
            if (questListPage != null &&
                questListPage.activeSelf)
            {
                LoadQuestsForCategory();
                RenderQuestPage();
            }
            else
            {
                RenderCategories();
            }
        }
    }
}