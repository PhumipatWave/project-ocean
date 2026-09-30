using System.Collections;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Plays an Auto Display Dialogue step's lines on the shared dialogue panel, one per Interact press.</summary>
    // The last press completes the step like a real NPC talk. On a Cutscene step it replaces the auto-play;
    // a line can still start a Timeline cutscene through its cutsceneId.
    public class PlayerSelfDialogueUI : MonoBehaviour
    {
        private const string PlayerId = "player";

        [Tooltip("Shown when a line's Character Id is blank or unknown to the CharacterDatabase.")]
        [SerializeField] private string displayName = "You";

        [Tooltip("DialoguePanelView on the dialogue panel (an IDialogueView).")]
        [SerializeField] private MonoBehaviour dialogueView;

        // Unity null check first: a destroyed view must read as missing.
        private IDialogueView View => dialogueView != null ? dialogueView as IDialogueView : null;

        // A dragged GameObject can land on the wrong component (e.g. its Image); keep the one implementing the interface.
        private void OnValidate()
        {
            if (dialogueView != null && !(dialogueView is IDialogueView))
                dialogueView = dialogueView.GetComponent<IDialogueView>() as MonoBehaviour;
        }

        private struct Line { public string speakerId; public string text; }

        private Quest activeQuest;
        private bool isShowing;
        private readonly List<Line> lines = new();
        private int lineIndex;

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            LoadingManager.OnLoadingFinished += Refresh;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
            LoadingManager.OnLoadingFinished -= Refresh;
            if (isShowing)
                Hide();
        }

        // Wait for every Awake (quests start there) and every NPC Start (it hides this shared panel) before opening.
        private void Start() => StartCoroutine(RefreshNextFrame());

        private IEnumerator RefreshNextFrame()
        {
            yield return null;
            Refresh();
        }

        private void Refresh()
        {
            // Wait for the loading screen; OnLoadingFinished calls Refresh again.
            if (LoadingManager.IsLoading) return;

            Quest match = null;

            if (QuestManager.Instance != null)
            {
                foreach (var q in QuestManager.Instance.GetActiveQuests())
                {
                    if (q.Status != QuestStatus.InProgress)
                        continue;

                    var step = q.CurrentStep;
                    if (step != null && (step.Type == QuestType.Talk || step.Type == QuestType.Cutscene) &&
                        step.AutoDisplayDialogue)
                    {
                        match = q;
                        break;
                    }
                }
            }

            if (match != null)
            {
                if (!isShowing || match != activeQuest)
                    Show(match);
            }
            else if (isShowing)
            {
                Hide();
            }
        }

        private void Show(Quest quest)
        {
            activeQuest = quest;
            isShowing = true;
            DialogueState.SetSelfDialogueShowing(true);

            lines.Clear();
            lineIndex = 0;
            foreach (var entry in quest.CurrentStep.DialogueEntries)
                foreach (var text in entry.lines ?? System.Array.Empty<string>())
                    lines.Add(new Line { speakerId = entry.characterId, text = text });

            // Nothing authored still needs one press to complete the step (as before).
            if (lines.Count == 0)
                lines.Add(new Line { speakerId = null, text = "" });

            ShowCurrentLine();
            View?.SetOpen(true);
            DialogueState.Show(View?.Panel);

            PlayerUIState.Open();

            var reader = PlayerRegistry.Local?.input;
            if (reader != null) reader.OnInteracted += HandleInteract;
        }

        private void ShowCurrentLine()
        {
            var line = lines[lineIndex];
            View?.SetSpeaker(string.IsNullOrEmpty(line.speakerId) ? PlayerId : line.speakerId, displayName);
            View?.SetText(line.text);
        }

        private void Hide()
        {
            isShowing = false;
            DialogueState.SetSelfDialogueShowing(false);
            activeQuest = null;

            View?.SetOpen(false);
            DialogueState.Hide(View?.Panel);

            var reader = PlayerRegistry.Local?.input;
            if (reader != null) reader.OnInteracted -= HandleInteract;

            PlayerUIState.Close();
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed || !isShowing || CutsceneGate.IsPlaying) return;

            if (lineIndex < lines.Count - 1)
            {
                lineIndex++;
                ShowCurrentLine();
                return;
            }

            // Captured before Hide() clears activeQuest — the cutscene starts once the
            // player has actually finished this exchange, not the moment it first showed.
            var quest = activeQuest;
            string cutsceneId = null;
            foreach (var entry in quest.CurrentStep.DialogueEntries)
            {
                if (string.IsNullOrEmpty(entry.cutsceneId)) continue;
                cutsceneId = entry.cutsceneId;
                break;
            }

            Hide();
            QuestManager.Instance?.CompleteAutoDialogue(quest);

            if (!string.IsNullOrEmpty(cutsceneId))
                CutsceneManager.Instance?.Play(cutsceneId);
        }
    }
}
