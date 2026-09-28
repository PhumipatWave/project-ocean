using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Completes a QuestType.ExploreArea step whose Area Id matches this one — either by the
    /// player interacting with this object (Interact) or by walking into its trigger collider
    /// (TriggerZone). Raises ExploreEvents.OnAreaReached; QuestManager only progresses a quest
    /// whose CURRENT step is that area, so reaching it before the step is active does nothing.
    ///
    /// Deliberately unguided — no compass arrow (QuestNavigation.ResolveQuestTarget skips
    /// ExploreArea) and no close-range marker of its own. Finding the area is the point of an
    /// "explore" step; navigating the player straight to it would defeat that.
    ///
    /// Interact mode only: if the currently-matching step has dialogue lines authored, pressing
    /// E shows them one at a time first (same plain-sequential model as PlayerSelfDialogueUI),
    /// and the area is only marked reached once the player has pressed through the last line —
    /// same "resolve only after they've actually read it" timing NPC/self-dialogue use. No
    /// dialogue authored (or no dialogueView assigned) skips straight to reached, unchanged.
    /// </summary>
    public class ExploreAreaTrigger : MonoBehaviour
    {
        public enum Mode
        {
            Interact,     // Player presses E on this object (needs an InteractUI on the same GameObject).
            TriggerZone,  // Player walks into this object's trigger Collider2D (tag Player).
        }

        [Tooltip("Matches an ExploreArea quest step's Area Id.")]
        [SerializeField] private string areaId;

        [Tooltip("Interact: player presses E on this object (needs an InteractUI on the same GameObject). TriggerZone: player walks into this object's trigger Collider2D.")]
        [SerializeField] private Mode mode;

        [Header("Dialogue (Interact only, optional)")]
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

        private struct DialogueLine { public string speakerId; public string text; }
        private readonly List<DialogueLine> dialogueLines = new();
        private int dialogueIndex;
        private bool isShowingDialogue;

        private static readonly List<ExploreAreaTrigger> all = new();

        /// <summary>The first registered trigger with this Area Id, or null if none is loaded.</summary>
        public static Transform FindByAreaId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var trigger in all)
                if (trigger.areaId == id)
                    return trigger.transform;
            return null;
        }

        private InteractUI interactUI;
        private bool playerInside;
        private bool recheck;

        private void Awake()
        {
            if (mode == Mode.Interact)
            {
                interactUI = GetComponent<InteractUI>();
                if (interactUI != null)
                    interactUI.onInteract.AddListener(HandleInteract);
                else
                    Debug.LogWarning($"[ExploreAreaTrigger] '{name}' is Interact mode but has no InteractUI on it.", this);
            }
            else
            {
                var zone = GetComponent<Collider2D>();
                if (zone == null || !zone.isTrigger)
                    Debug.LogWarning($"[ExploreAreaTrigger] '{name}' is TriggerZone mode but has no trigger Collider2D on it.", this);
            }
        }

        private void OnDestroy()
        {
            if (interactUI != null)
                interactUI.onInteract.RemoveListener(HandleInteract);
        }

        private void OnEnable()
        {
            all.Add(this);
            QuestManager.OnQuestUpdated += HandleQuestUpdated;
        }

        private void OnDisable()
        {
            all.Remove(this);
            QuestManager.OnQuestUpdated -= HandleQuestUpdated;
            playerInside = false;
            if (isShowingDialogue) CloseDialogue();
        }

        // InteractUI has already taken the player lock for this press. A matching step with
        // dialogue authored shows/advances it instead of reporting immediately — the lock stays
        // held (InteractUI still treats further presses here as ours) until the last line closes
        // it. No dialogue (or none authored for the current step) reports immediately, unchanged.
        private void HandleInteract()
        {
            if (isShowingDialogue)
            {
                AdvanceDialogue();
                return;
            }

            var step = FindCurrentStep();
            if (View?.Panel != null && step != null && step.DialogueEntries.Count > 0)
            {
                OpenDialogue(step);
                return;
            }

            interactUI.ReleasePlayerLock();
            Raise();
        }

        /// <summary>The InProgress quest (if any) whose current step is THIS area — same match
        /// QuestManager.HandleAreaReached uses, so the dialogue shown is always whichever step
        /// Raise() below would actually complete.</summary>
        private QuestStep FindCurrentStep()
        {
            if (string.IsNullOrEmpty(areaId) || QuestManager.Instance == null)
                return null;

            foreach (var q in QuestManager.Instance.GetActiveQuests())
            {
                if (q.Status != QuestStatus.InProgress) continue;
                var step = q.CurrentStep;
                if (step != null && step.Type == QuestType.ExploreArea && step.EffectiveAreaId == areaId)
                    return step;
            }
            return null;
        }

        private void OpenDialogue(QuestStep step)
        {
            dialogueLines.Clear();
            dialogueIndex = 0;
            foreach (var entry in step.DialogueEntries)
                foreach (var text in entry.lines ?? System.Array.Empty<string>())
                    dialogueLines.Add(new DialogueLine { speakerId = entry.characterId, text = text });

            isShowingDialogue = true;
            ShowCurrentLine();
            View?.SetOpen(true);
            DialogueState.Show(View?.Panel);
        }

        private void ShowCurrentLine()
        {
            var line = dialogueLines[dialogueIndex];
            View?.SetSpeaker(line.speakerId);
            View?.SetText(line.text);
        }

        private void AdvanceDialogue()
        {
            if (dialogueIndex < dialogueLines.Count - 1)
            {
                dialogueIndex++;
                ShowCurrentLine();
                return;
            }

            CloseDialogue();
            interactUI.ReleasePlayerLock();
            Raise();
        }

        private void CloseDialogue()
        {
            isShowingDialogue = false;
            var view = View;
            if (view == null || view.Panel == null) return;
            view.SetOpen(false);
            DialogueState.Hide(view.Panel);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (mode != Mode.TriggerZone || !other.CompareTag("Player")) return;

            playerInside = true;
            recheck = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            // Walking away mid-conversation closes it (the InteractUI on this same object
            // releases the player lock itself via its own OnTriggerExit2D) — don't leave the
            // panel stuck open with no lock holding it.
            if (mode == Mode.Interact)
            {
                if (isShowingDialogue) CloseDialogue();
                return;
            }

            playerInside = false;
        }

        // A step can become current while the player is already standing inside (Enter won't
        // fire again). Only flag it here — raising straight from OnQuestUpdated would re-enter
        // QuestManager while it is still iterating its quest list.
        private void HandleQuestUpdated()
        {
            if (playerInside) recheck = true;
        }

        private void Update()
        {
            // Held back while a cutscene plays (it may walk the player through the zone), then
            // fires once it is over if they are still inside.
            if (recheck && playerInside && !CutsceneGate.IsPlaying)
            {
                recheck = false;
                Raise();
            }
        }

        private void Raise()
        {
            if (!string.IsNullOrEmpty(areaId))
                ExploreEvents.RaiseAreaReached(areaId);
        }
    }
}
