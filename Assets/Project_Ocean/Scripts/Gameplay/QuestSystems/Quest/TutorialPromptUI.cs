using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Shows the prompt for the current guided step and sets TutorialGate to require its action.</summary>
    // Guided is per step. Scans all active quests, so changing the tracked quest can't hide an unfinished tutorial.
    // Hides under any other blocking panel but never takes the UI lock itself.
    public class TutorialPromptUI : MonoBehaviour
    {
        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            PlayerUIState.OnLockChanged += HandleLockChanged;
            LoadingManager.OnLoadingFinished += Refresh;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
            PlayerUIState.OnLockChanged -= HandleLockChanged;
            LoadingManager.OnLoadingFinished -= Refresh;

            Hide();
        }

        private void Start() => Refresh();

        private bool pausingForUseItem;

        // Guided "Use MiniHub" step freezes the ocean until the hub is placed. Re-applied every frame because
        // aim, UIManager and chests also write timeScale. ponytail: scans quests per frame; cache on OnQuestUpdated if slow.
        private void LateUpdate()
        {
            bool shouldPause = TutorialStep.IsActive(TutorialActionId.UseItem)
                && GameSession.State == GameState.Ocean
                && !LoadingManager.IsLoading
                && Inventory.Current != null
                && Inventory.Current.PeekItem(ItemType.Building) != null; // no Mini Hub = no freeze (soft-lock guard)

            if (shouldPause)
            {
                Time.timeScale = 0f;
                pausingForUseItem = true;
            }
            else if (pausingForUseItem)
            {
                pausingForUseItem = false;
                if (!PlayerUIState.IsLocked) Time.timeScale = 1f; // an open pause menu keeps its own 0
            }
        }

        private void HandleLockChanged(bool locked) => Refresh();

        private void Refresh()
        {
            // Quests stay active in memory back at the main menu — no prompt there.
            if (PlayerUIState.IsLocked || QuestManager.Instance == null || LoadingManager.IsLoading
                || SceneManager.GetActiveScene().name == SceneNames.MainMenu)
            {
                Hide();
                return;
            }

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                if (!quest.IsGuidedSequence) continue;

                var step = quest.CurrentStep;
                if (step == null) continue;

                if (step.Type == QuestType.Tutorial)
                    Show(quest.StepDescription, step.NotificationImage, step.TutorialAction);
                else if (step.Type == QuestType.Talk)
                    Show(quest.StepDescription, step.NotificationImage, TutorialActionId.Interact);
                else
                    continue;

                return;
            }

            Hide();
        }

        private void Show(string text, Sprite image, TutorialActionId requiredAction)
        {
            // Refresh runs on many events; only toast when the prompt actually changes.
            bool changed = text != TutorialGate.CurrentPrompt;
            TutorialGate.Set(requiredAction, text);
            if (changed)
                NotificationManager.Notify(text, NotificationChannel.Center, image);
        }

        private void Hide()
        {
            if (TutorialGate.RequiredAction.HasValue)
                GameLog.Info($"[TutorialPromptUI] Hide — was requiring {TutorialGate.RequiredAction.Value} " +
                    $"(locked={PlayerUIState.IsLocked}, questManagerNull={QuestManager.Instance == null})");

            TutorialGate.Clear();
        }
    }
}
