using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    /// <summary>
    /// Drives the current step's instruction text for whichever active quest's CURRENT
    /// checkpoint is authored as guided (QuestStepData.isGuidedSequence →
    /// QuestStep.IsGuidedSequence, surfaced quest-wide via Quest.IsGuidedSequence), and
    /// clears it once no such step is active. Guided is per-checkpoint, not per-quest — a
    /// Tutorial step's own action, or a Talk step, is only gated/prompted if THAT step has
    /// the flag set. A step with isGuidedSequence left off is never treated as guided — no
    /// prompt, no input restriction.
    /// Scans active quests directly (not GetTrackedQuest()) so switching the player's
    /// tracked quest can never hide a tutorial prompt they haven't finished yet.
    /// Sets TutorialGate.RequiredAction while showing (restricting the player to just
    /// that action — ESC/pause stays available) and clears it when the prompt hides.
    /// Hides itself under any OTHER blocking panel (dialogue, shop, menu) rather than
    /// showing over it, but never sets PlayerUIState's lock itself.
    /// Shows the prompt as a Center-channel toast; decision logic only, no UI of its own.
    /// </summary>
    public class TutorialPromptUI : MonoBehaviour
    {
        /// <summary>Prompt text currently in effect, or null — re-shown when the player presses a blocked key.</summary>
        public static string CurrentPrompt { get; private set; }

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

        // Guided "Use MiniHub" step in the ocean: freeze the game (breach countdown, fish, movement)
        // until the player places it. Re-asserted every frame — aim, UIManager and chests all write
        // timeScale. ponytail: scans active quests per frame; cache on OnQuestUpdated if quests grow.
        private void LateUpdate()
        {
            bool shouldPause = TutorialStep.IsActive(TutorialActionId.UseItem)
                && GameManager.Instance != null && GameManager.Instance.GameState == GameState.Ocean
                && !LoadingManager.IsLoading
                && InventoryManager.Instance != null
                && InventoryManager.Instance.PeekItem(ItemType.Building) != null; // no Mini Hub = no freeze (soft-lock guard)

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
            if (text != CurrentPrompt)
            {
                CurrentPrompt = text;
                NotificationManager.Notify(text, NotificationChannel.Center, image);
            }
            TutorialGate.RequiredAction = requiredAction;
        }

        private void Hide()
        {
            if (TutorialGate.RequiredAction.HasValue)
                Debug.Log($"[TutorialPromptUI] Hide — was requiring {TutorialGate.RequiredAction.Value} " +
                    $"(locked={PlayerUIState.IsLocked}, questManagerNull={QuestManager.Instance == null})");

            CurrentPrompt = null;
            TutorialGate.RequiredAction = null;
        }
    }
}
