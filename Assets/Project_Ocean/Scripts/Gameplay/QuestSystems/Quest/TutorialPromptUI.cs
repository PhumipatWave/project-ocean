using TMPro;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shows the current step's instruction text for whichever active quest is a guided
    /// sequence (Quest.HasTutorialStep — has a Tutorial step somewhere in its chain), and
    /// hides once no such step is active. This covers a Tutorial step's own action, and
    /// also a Talk step that comes AFTER a Tutorial step in the same chain (so "next goal"
    /// keeps showing across the whole guided quest, not just its literal Tutorial steps).
    /// A standalone Talk/KillCount/etc. quest with no Tutorial step anywhere in it is never
    /// treated as guided — no popup, no input restriction, same as any normal quest.
    /// Scans active quests directly (not GetTrackedQuest()) so switching the player's
    /// tracked quest can never hide a tutorial prompt they haven't finished yet.
    /// Sets TutorialGate.RequiredAction while visible (restricting the player to just
    /// that action — ESC/pause stays available) and clears it when the prompt hides.
    /// Hides itself under any OTHER blocking panel (dialogue, shop, menu) rather than
    /// showing over it, but never sets PlayerUIState's lock itself.
    /// </summary>
    public class TutorialPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI promptText;

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            PlayerUIState.OnLockChanged += HandleLockChanged;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
            PlayerUIState.OnLockChanged -= HandleLockChanged;

            TutorialGate.RequiredAction = null;
        }

        private void Start() => Refresh();

        private void HandleLockChanged(bool locked) => Refresh();

        private void Refresh()
        {
            if (PlayerUIState.IsLocked || QuestManager.Instance == null)
            {
                Hide();
                return;
            }

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                if (!quest.HasTutorialStep) continue;

                var step = quest.CurrentStep;
                if (step == null) continue;

                if (step.Type == QuestType.Tutorial)
                    Show(quest.StepDescription, step.TutorialAction);
                else if (step.Type == QuestType.Talk)
                    Show(quest.StepDescription, TutorialActionId.Interact);
                else
                    continue;

                return;
            }

            Hide();
        }

        private void Show(string text, TutorialActionId requiredAction)
        {
            if (panel != null) panel.SetActive(true);
            if (promptText != null) promptText.text = text;
            TutorialGate.RequiredAction = requiredAction;
        }

        private void Hide()
        {
            if (panel != null) panel.SetActive(false);
            TutorialGate.RequiredAction = null;
        }
    }
}
