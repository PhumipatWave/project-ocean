using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
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
        // Typed as MonoBehaviour (an IQuestOfferView) so gameplay doesn't reference the UI class; same name keeps the reference.
        [SerializeField] private MonoBehaviour questDetailPanel;

        [Header("Dialogue UI")]
        [Tooltip("DialoguePanelView on the dialogue panel (an IDialogueView).")]
        [SerializeField] private MonoBehaviour dialogueView;

        [Header("Quest Marker")]
        [Tooltip("Shown while this NPC is the target of the active quest's current Talk step (and, for a not-yet-accepted quest, only once its prerequisite is unlocked — same gate as the quest conversation).")]
        [SerializeField] private GameObject markerUI;

        public string NpcId => npcId;
        public string NpcName => npcName;
        public Sprite NpcPortrait => npcPortrait;

        // Unity null check first: a destroyed view must read as missing.
        internal IDialogueView View => dialogueView != null ? dialogueView as IDialogueView : null;
        internal IQuestOfferView QuestOfferView => questDetailPanel != null ? questDetailPanel as IQuestOfferView : null;

        // A Main quest's Talk step targeting this NPC counts even without its own targetQuest.
        private bool HasQuest =>
            targetQuest != null ||
            (QuestManager.Instance != null && QuestManager.Instance.GetMainQuestTalkQuest(npcId) != null);

        private NpcQuestMarker questMarker;
        private NpcQuestConversation questConversation;
        private bool isUIOpen;
        private InteractUI interactUI;

        // A dragged GameObject can land on the wrong component (e.g. its Image); keep the one implementing the interface.
        private void OnValidate()
        {
            if (dialogueView != null && !(dialogueView is IDialogueView))
                dialogueView = dialogueView.GetComponent<IDialogueView>() as MonoBehaviour;
            if (questDetailPanel != null && !(questDetailPanel is IQuestOfferView))
                questDetailPanel = questDetailPanel.GetComponent<IQuestOfferView>() as MonoBehaviour;
        }

        private void Awake()
        {
            questMarker = new NpcQuestMarker(this, markerUI);
            questConversation = new NpcQuestConversation(this);
        }

        private void OnEnable()
        {
            NPCManager.Instance?.Register(this);
            QuestManager.OnQuestUpdated += questMarker.Refresh;
        }

        private void OnDisable()
        {
            NPCManager.Instance?.Unregister(this);
            QuestManager.OnQuestUpdated -= questMarker.Refresh;
        }

        private void Start()
        {
            // InteractUI may sit on a child (the prefab's "Talk" object); without it the player lock never releases.
            interactUI = GetComponentInChildren<InteractUI>();

            // A child's trigger zone never calls this object's OnTriggerExit2D, so listen through InteractUI.
            if (interactUI != null && interactUI.gameObject != gameObject)
                interactUI.PlayerExited += HandlePlayerExited;

            View?.SetOpen(false);

            questMarker.Refresh();
        }

        // Proximity changes every frame; the quest part of the condition only on OnQuestUpdated.
        private void Update()
        {
            questMarker.Refresh();
        }

        /// <summary>The quest this NPC speaks for: a Main quest's Talk step first, then its own targetQuest.</summary>
        internal Quest ResolveActiveQuest()
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
                questConversation.Interact();
            else
                ToggleDialogue();
        }

        private void ToggleDialogue()
        {
            if (View == null || !View.IsOpen)
            {
                OpenDialogue();
                return;
            }

            CloseDialogue();
            // Another quest's Talk step may still be waiting on this npcId.
            RaiseTalkedTo();
        }

        private void OpenDialogue()
        {
            if (View == null || View.Panel == null)
                return;

            View.SetText(npcDialogue);
            ShowPanel();
        }

        internal void ShowPanel()
        {
            View?.SetSpeaker(npcId, npcName, npcPortrait);
            SetDialoguePanel(true);
            isUIOpen = true;
        }

        internal void RaiseTalkedTo()
        {
            if (!string.IsNullOrEmpty(npcId))
                NpcEvents.RaiseNpcTalkedTo(npcId);
        }

        // Also reports to DialogueState so other land UI (shop, board...) hides while it shows.
        private void SetDialoguePanel(bool show)
        {
            var view = View;
            if (view == null || view.Panel == null) return;
            view.SetOpen(show);
            if (show) DialogueState.Show(view.Panel);
            else DialogueState.Hide(view.Panel);
        }

        internal void CloseDialogue()
        {
            questConversation.End();
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

            if (HasQuest)
                QuestOfferView?.HidePanel();
        }
    }
}
