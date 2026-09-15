using System;
using System.Collections;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>
    /// Drives the shared NPC dialogue panel for a Talk step whose Target NPC Id is "player" —
    /// there's no NPC to walk up to and press Interact on, so the line auto-opens the moment
    /// that step becomes current, and one Interact press closes it and completes the step via
    /// the same NpcEvents pipeline a real NPC conversation uses (rewards/next-step/chaining
    /// all keep working unchanged).
    /// </summary>
    public class PlayerSelfDialogueUI : MonoBehaviour
    {
        private const string SelfNpcId = "player";

        [SerializeField] private string displayName = "You";

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image npcAvatarImage;

        private Quest activeQuest;
        private bool isShowing;

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= Refresh;
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
            Quest match = null;

            if (QuestManager.Instance != null)
            {
                foreach (var q in QuestManager.Instance.GetActiveQuests())
                {
                    if (q.Status != QuestStatus.InProgress)
                        continue;

                    var step = q.CurrentStep;
                    if (step != null && step.Type == QuestType.Talk &&
                        string.Equals(step.TargetEnemyId, SelfNpcId, StringComparison.OrdinalIgnoreCase))
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

            if (npcNameText != null) npcNameText.text = displayName;
            if (npcAvatarImage != null) npcAvatarImage.gameObject.SetActive(false);
            if (dialogueText != null) dialogueText.text = quest.CurrentStep.QuestionInProgress;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            PlayerUIState.Open();

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted += HandleInteract;
        }

        private void Hide()
        {
            isShowing = false;
            activeQuest = null;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted -= HandleInteract;

            PlayerUIState.Close();
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed || !isShowing) return;

            Hide();
            NpcEvents.RaiseNpcTalkedTo(SelfNpcId);
        }
    }
}
