using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Testing
{
    /// <summary>Raised whenever the player interacts with an NPC. Payload = that NPC's npcId.</summary>
    public static class NpcEvents
    {
        public static event System.Action<string> OnNpcTalkedTo;

        public static void RaiseNpcTalkedTo(string npcId)
        {
            OnNpcTalkedTo?.Invoke(npcId);
        }
    }

    public class NPC : MonoBehaviour
    {
        [Header("NPC Settings")]
        [Tooltip("Stable id for this NPC, matched by Talk-type quest steps (Target NPC Id). Leave blank if no quest ever needs to target this specific NPC.")]
        [SerializeField] private string npcId;

        [Header("Normal Dialogue")]
        [SerializeField] private string npcName;
        [TextArea(3, 10)]
        [SerializeField] private string npcDialogue = "...";
        [SerializeField] private Sprite npcPortrait;

        [Header("Quest Settings")]
        [Tooltip("Leave empty for an NPC with no quest — falls back to plain dialogue.")]
        [SerializeField] private QuestData targetQuest;
        [SerializeField] private QuestDetailPanel questDetailPanel;

        // A Main quest's Talk step targeting this NPC (from ANY active quest) counts even
        // if this NPC has no targetQuest of its own — see ResolveActiveQuest.
        private bool HasQuest =>
            targetQuest != null ||
            (QuestManager.Instance != null && QuestManager.Instance.GetMainQuestTalkQuest(npcId) != null);

        [Header("Dialogue UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image npcAvatarImage;

        [Header("Quest Marker")]
        [Tooltip("Shown while this NPC is the target of the active quest's current Talk step (and, for a not-yet-accepted quest, only once its prerequisite is unlocked — same gate as StartDialogue).")]
        [SerializeField] private GameObject markerUI;

        public string NpcId => npcId;
        public string NpcName => npcName;
        public Sprite NpcPortrait => npcPortrait;

        private Quest npcQuest;
        private int currentDialogueIndex;
        private bool isConversing;
        private bool isUIOpen;
        private InteractUI interactUI;

        private void OnEnable()
        {
            NPCManager.Instance?.Register(this);
            QuestManager.OnQuestUpdated += RefreshMarker;
        }

        private void OnDisable()
        {
            NPCManager.Instance?.Unregister(this);
            QuestManager.OnQuestUpdated -= RefreshMarker;
        }

        private void Start()
        {
            // The InteractUI (and its trigger collider) may sit on a child (e.g. the prefab's
            // "Talk" object) rather than on the NPC itself — find it either way, or the
            // player lock never gets released when the dialogue closes.
            interactUI = GetComponentInChildren<InteractUI>();

            // Trigger events go to the collider's own object, so an NPC whose zone is on a
            // child never gets its own OnTriggerExit2D — listen through the InteractUI instead.
            if (interactUI != null && interactUI.gameObject != gameObject)
                interactUI.PlayerExited += HandlePlayerExited;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            RefreshMarker();
        }

        /// <summary>True while this NPC is the active quest's current Talk-step target — mirrors
        /// StartDialogue's own unlock gate for a not-yet-accepted (New) quest — and the player
        /// is close enough that QuestCompass has handed off to this marker (see
        /// QuestNavigation.CloseRangeMeters).</summary>
        private bool ShouldShowMarker()
        {
            var quest = ResolveActiveQuest();
            var step = quest?.CurrentStep;
            if (step == null || step.Type != QuestType.Talk || step.EffectiveNpcId != npcId)
                return false;

            bool targeted = quest.Status switch
            {
                QuestStatus.New => QuestManager.Instance != null && QuestManager.Instance.IsUnlocked(quest),
                QuestStatus.InProgress or QuestStatus.ReadyToTurnIn => true,
                _ => false,
            };
            if (!targeted)
                return false;

            var player = GameManager.CurPlayer;
            return player != null &&
                Vector3.Distance(player.transform.position, QuestNavigation.InteractPosition(transform)) <= QuestNavigation.CloseRangeMeters;
        }

        private void RefreshMarker()
        {
            if (markerUI != null)
                markerUI.SetActive(ShouldShowMarker());
        }

        // Proximity changes every frame as the player walks, unlike the rest of the marker's
        // condition (which only changes on OnQuestUpdated) — skip entirely for the many NPCs
        // that never carry a marker (see the dummy "locked door" NPCs) or aren't a live target.
        private void Update()
        {
            if (markerUI != null && HasQuest)
                RefreshMarker();
        }

        /// <summary>
        /// Which quest this NPC currently speaks for. A Main quest's Talk step targeting
        /// this NPC (from any active quest) takes priority over the NPC's own assigned
        /// targetQuest — main-story dialogue always plays first, regardless of that step's
        /// IsGuidedSequence (that flag only controls the tutorial prompt/input-lock, not
        /// which dialogue is shown — see TutorialPromptUI). Resolved fresh each
        /// conversation, not cached, so it stays correct as the Main quest advances.
        /// </summary>
        private Quest ResolveActiveQuest()
        {
            if (QuestManager.Instance == null)
                return null;

            var mainQuest = QuestManager.Instance.GetMainQuestTalkQuest(npcId);
            if (mainQuest != null)
                return mainQuest;

            return targetQuest != null ? QuestManager.Instance.GetQuestById(targetQuest.questId) : null;
        }

        public void Interact()
        {
            if (CutsceneGate.IsPlaying || LoadingManager.IsLoading) return;

            if (HasQuest)
                QuestInteract();
            else
                ToggleDialogue();
        }

        private void ToggleDialogue()
        {
            bool wasOpen = dialoguePanel != null && dialoguePanel.activeSelf;

            if (!wasOpen)
                OpenDialogue();
            else
                CloseDialogue();

            // Fire only on the close (the player has now actually seen the line) — not
            // this NPC's own quest, but another quest's (non-guided) Talk step may still
            // be waiting on this npcId, so keep progressing that in the background.
            if (wasOpen && !string.IsNullOrEmpty(npcId))
                NpcEvents.RaiseNpcTalkedTo(npcId);
        }

        private void OpenDialogue()
        {
            if (dialoguePanel == null)
                return;

            if (dialogueText != null)
                dialogueText.text = npcDialogue;

            DialogueSpeaker.Apply(npcNameText, npcAvatarImage, npcId, npcName, npcPortrait);
            SetDialoguePanel(true);
            isUIOpen = true;
        }

        // Also reports to DialogueState so other land UI (shop, board...) hides while it shows.
        private void SetDialoguePanel(bool show)
        {
            if (dialoguePanel == null) return;
            dialoguePanel.SetActive(show);
            if (show) DialogueState.Show(dialoguePanel);
            else DialogueState.Hide(dialoguePanel);
        }

        private void QuestInteract()
        {
            if (!isConversing)
                StartDialogue();
            else
                AdvanceDialogue();
        }

        private void StartDialogue()
        {
            npcQuest = ResolveActiveQuest();
            if (npcQuest == null)
                return;

            // A New quest with an unfinished prerequisite isn't offered yet.
            if (npcQuest.Status == QuestStatus.New &&
                QuestManager.Instance != null && !QuestManager.Instance.IsUnlocked(npcQuest))
                return;

            isConversing = true;
            currentDialogueIndex = 0;

            SetDialoguePanel(true);

            isUIOpen = true;

            // Show THIS physical NPC's own identity, not the quest's — several NPCs can
            // share one targetQuest (e.g. different Talk steps of the same quest), and
            // each should be labeled as themselves when the player talks to them.
            DialogueSpeaker.Apply(npcNameText, npcAvatarImage, npcId, npcName, npcPortrait);

            ShowDialogueText();
        }

        private void ShowDialogueText()
        {
            if (dialogueText == null)
                return;

            switch (npcQuest.Status)
            {
                case QuestStatus.New:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.Offer, currentDialogueIndex);
                    break;
                case QuestStatus.InProgress:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.InProgress);
                    break;
                // Must show the Finish line here, not fall through blank — the player is
                // about to turn the quest in, not still working toward it.
                case QuestStatus.ReadyToTurnIn:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.Finish);
                    break;
                case QuestStatus.Finish:
                    dialogueText.text = npcQuest.CurrentStep.GetLine(npcId, DialogueType.Finish);
                    break;
                case QuestStatus.Reject:
                    dialogueText.text = "?";
                    break;
            }
        }

        /// <summary>
        /// Plays this line's cutscene (QuestDialogueLine.cutsceneId), if one is authored.
        /// Called from AdvanceDialogue's closing branches (after CloseDialogue), not from
        /// ShowDialogueText — the cutscene should start once the player has actually finished
        /// this exchange, not the moment the line first appears.
        /// </summary>
        private void PlayCutsceneIfAny(DialogueType type)
        {
            var id = npcQuest.CurrentStep.GetCutsceneId(npcId, type);
            if (!string.IsNullOrEmpty(id))
                CutsceneManager.Instance?.Play(id);
        }

        private void AdvanceDialogue()
        {
            if (npcQuest.Status == QuestStatus.New)
            {
                currentDialogueIndex++;
                var offerLines = npcQuest.CurrentStep.GetLines(npcId, DialogueType.Offer);
                if (currentDialogueIndex < offerLines.Length)
                    dialogueText.text = offerLines[currentDialogueIndex];
                else
                {
                    // Player has now read this NPC's full greeting — other quests' Talk
                    // steps targeting this npcId (e.g. a background tutorial step) can
                    // progress now, same as any other "finished talking" moment below.
                    if (!string.IsNullOrEmpty(npcId))
                        NpcEvents.RaiseNpcTalkedTo(npcId);

                    CloseDialogue();
                    PlayCutsceneIfAny(DialogueType.Offer);

                    if (questDetailPanel != null)
                        questDetailPanel.ShowPanel(npcQuest, null);
                }
            }
            // Must be handled explicitly, not left to fall into the generic else branch below —
            // that branch only closes the dialogue, it never calls TurnInQuest, so the quest
            // would never actually complete.
            else if (npcQuest.Status == QuestStatus.ReadyToTurnIn)
            {
                if (!string.IsNullOrEmpty(npcId))
                    NpcEvents.RaiseNpcTalkedTo(npcId);

                CloseDialogue();
                PlayCutsceneIfAny(DialogueType.Finish);

                bool success = QuestManager.Instance.TurnInQuest(npcQuest.QuestID);
                if (!success)
                    Debug.LogWarning($"[NPCInteract] TurnInQuest failed for id {npcQuest.QuestID}");
            }
            else
            {
                // The player has now actually seen this step's line (shown on the previous
                // press) — only NOW does the Talk step get to progress, not the moment the
                // conversation was opened. Harmless to raise if the quest already isn't
                // InProgress (Finish/Reject) — QuestManager only matches InProgress quests.
                if (!string.IsNullOrEmpty(npcId))
                    NpcEvents.RaiseNpcTalkedTo(npcId);

                CloseDialogue();
                PlayCutsceneIfAny(npcQuest.Status == QuestStatus.Finish ? DialogueType.Finish : DialogueType.InProgress);
            }
        }

        private void CloseDialogue()
        {
            isConversing = false;
            SetDialoguePanel(false);

            if (isUIOpen)
            {
                interactUI?.ReleasePlayerLock();
                isUIOpen = false;
            }
        }

        private void OnDestroy()
        {
            if (interactUI != null)
                interactUI.PlayerExited -= HandlePlayerExited;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                HandlePlayerExited();
        }

        private void HandlePlayerExited()
        {
            CloseDialogue();

            if (HasQuest && questDetailPanel != null)
                questDetailPanel.HidePanel();
        }
    }
}
