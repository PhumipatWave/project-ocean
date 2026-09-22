using UnityEngine;

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
    ///
    /// Rendering is delegated to GameHUD's shared NotificationTextUI (via
    /// NotificationManager.ShowPersistent/ClearPersistent) rather than a dedicated panel —
    /// this is decision-logic only, no UI of its own.
    /// </summary>
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

        private void HandleLockChanged(bool locked) => Refresh();

        private void Refresh()
        {
            if (PlayerUIState.IsLocked || QuestManager.Instance == null || LoadingManager.IsLoading)
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
            NotificationManager.ShowPersistent(text, image);
            TutorialGate.RequiredAction = requiredAction;
        }

        private void Hide()
        {
            if (TutorialGate.RequiredAction.HasValue)
                Debug.Log($"[TutorialPromptUI] Hide — was requiring {TutorialGate.RequiredAction.Value} " +
                    $"(locked={PlayerUIState.IsLocked}, questManagerNull={QuestManager.Instance == null})");

            NotificationManager.ClearPersistent();
            TutorialGate.RequiredAction = null;
        }
    }
}
