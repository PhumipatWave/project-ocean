using System.Collections;
using System.Collections.Generic;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>
    /// Drives the shared NPC dialogue panel for a Talk or Cutscene step authored with Auto
    /// Display Dialogue (QuestStepData.autoDisplayDialogue) — there's no NPC to walk up to and
    /// press Interact on, so the line auto-opens the moment that step becomes current. Plays
    /// the step's dialogue entries (any type) in authored order, one line per Interact press,
    /// each with its own Character Id's name and portrait (blank = the player, "You"); the last
    /// press closes it and completes the step via QuestManager.CompleteAutoDialogue (same
    /// CompleteProgress a real NPC conversation ends in, so rewards/next-step/chaining keep
    /// working unchanged). On a Cutscene step this replaces CutsceneManager's own auto-play —
    /// see its RefreshQuestStep guard — so a "cinematic" beat can instead be driven by the
    /// player pressing through dialogue, optionally still chaining into a real Timeline
    /// cutscene via a line's own cutsceneId (see HandleInteract below).
    /// </summary>
    public class PlayerSelfDialogueUI : MonoBehaviour
    {
        private const string PlayerId = "player";

        [Tooltip("Shown when a line's Character Id is blank or unknown to the CharacterDatabase.")]
        [SerializeField] private string displayName = "You";

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image npcAvatarImage;

        private struct Line { public string speakerId; public string text; }

        private static bool showing;
        private static int lastHideFrame = -1;

        /// <summary>True while an auto-display dialogue is up, and for the rest of the frame it
        /// closes on — every Interact listener (InteractUI, collectables) is handling that SAME
        /// press, and the one that closes the dialogue must not also start a shop/door/pickup.
        /// InteractUI alone can't rely on the player lock: an interactable that already holds it
        /// (the shop the player just left) would still act on the press.</summary>
        public static bool BlocksInteraction => showing || Time.frameCount <= lastHideFrame;

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

        // QuestManager.InitializeQuests() runs in ITS Awake — a quest authored to start
        // already InProgress (like MQ_1001) never fires OnQuestUpdated, so OnEnable's
        // Refresh() could race Awake ordering and miss it. Start() is guaranteed to run
        // after every object's Awake/OnEnable, so QuestManager.Instance is ready here —
        // same fix TutorialPromptUI already uses for the identical race. One frame further:
        // every NPC's own Start() resets the SAME shared dialoguePanel to hidden as a safety
        // default, and Start() order between sibling GameObjects isn't guaranteed — waiting a
        // frame guarantees every NPC's reset has already run before we open the panel.
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
            showing = true;

            lines.Clear();
            lineIndex = 0;
            foreach (var entry in quest.CurrentStep.DialogueEntries)
                foreach (var text in entry.lines ?? System.Array.Empty<string>())
                    lines.Add(new Line { speakerId = entry.characterId, text = text });

            // Nothing authored still needs one press to complete the step (as before).
            if (lines.Count == 0)
                lines.Add(new Line { speakerId = null, text = "" });

            ShowCurrentLine();
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            DialogueState.Show(dialoguePanel);

            PlayerUIState.Open();

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted += HandleInteract;
        }

        private void ShowCurrentLine()
        {
            var line = lines[lineIndex];
            DialogueSpeaker.Apply(npcNameText, npcAvatarImage,
                string.IsNullOrEmpty(line.speakerId) ? PlayerId : line.speakerId, displayName);
            if (dialogueText != null) dialogueText.text = line.text;
        }

        private void Hide()
        {
            isShowing = false;
            showing = false;
            lastHideFrame = Time.frameCount;
            activeQuest = null;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            DialogueState.Hide(dialoguePanel);

            var reader = GameManager.CurPlayer?.input;
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
