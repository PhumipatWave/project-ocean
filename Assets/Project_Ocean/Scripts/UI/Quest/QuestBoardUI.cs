using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
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

        [Tooltip("Avatar shown on a quest row that has no quest-giver portrait.")]
        [SerializeField] private Sprite questBoardAvatar;

        [Header("Quest List Title")]
        [SerializeField] private TextMeshProUGUI questCategoryTitle;

        [Header("Navigation")]
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button backToCategoryButton;
        [SerializeField] private Button closeBoardButton;

        [Header("Quest Detail")]
        [SerializeField] private QuestDetailPanel questDetailPanel;

        [Header("Quest Board Root")]
        [SerializeField] private GameObject questBoardPanel;

        [Header("Category Settings")]
        [SerializeField] private QuestCategoryUIData[] categories;

        private List<Quest> questsInCurrentCategory =
            new List<Quest>();

        private QuestCategory currentCategory;

        private int currentPageIndex = 0;

        // Derived from the actual slot objects instead of a hardcoded number,
        // so paging always matches however many QuestSlots are in the scene.
        private int QuestsPerPage => questSlots.Length;

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
                    CloseBoard
                );
        }

        // Public so the Close button can call this directly from the Inspector.
        // This closes both the Quest Board and its PlayerUIState lock.
        public void CloseBoard()
        {
            if (questBoardPanel != null)
            {
                questBoardPanel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }

            PlayerUIState.Close();
        }

        private void OnEnable()
        {
            currentPageIndex = 0;

            if (QuestManager.Instance != null)
            {
                QuestManager.OnQuestUpdated += RefreshCurrentPage;
            }

            // The InteractUI that revealed this board already called PlayerUIState.Open().
            // Register the FULL close action so ESC also releases the PlayerUIState lock.
            PlayerUIState.SetCloser(CloseBoard);

            TutorialEvents.RaiseAction(TutorialActionId.OpenQuestBoard);

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

            // Reset the title back to "Quest Board".
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

            // Set the header title for this category.
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
                currentPageIndex * QuestsPerPage;

            for (int i = 0; i < questSlots.Length; i++)
            {
                int targetIndex = startingIndex + i;

                if (targetIndex < questsInCurrentCategory.Count)
                {
                    questSlots[i].SetupSlot(
                        questsInCurrentCategory[targetIndex],
                        OnQuestSlotSelected,
                        questBoardAvatar
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
                    startingIndex + QuestsPerPage
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
            if ((currentPageIndex + 1) * QuestsPerPage
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