using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Registry of all currently-loaded Cutscene instances, keyed by cutsceneId. Cutscenes
    /// self-register/unregister (OnEnable/OnDisable) rather than being scanned via
    /// FindObjectOfType, so the registry stays correct across additive scene loads and
    /// unloads — mirrors NPCManager, since QuestData (a ScriptableObject) can only ever
    /// store a cutsceneId, never a direct reference to a scene object.
    ///
    /// Also drives QuestType.Cutscene steps directly: unlike a cutscene attached to one
    /// Talk line (triggered by NPC.cs/PlayerSelfDialogueUI when that line shows), a
    /// Cutscene-type step has no dialogue to hang off of — it plays the moment it becomes
    /// the current step and completes itself via CutsceneEvents when done.
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        // A Cutscene-type step becomes current the instant the PREVIOUS step's completion
        // event fires — e.g. NPC.AdvanceDialogue() raises NpcEvents.RaiseNpcTalkedTo before
        // it even calls CloseDialogue(). Without this delay the cutscene would snatch control
        // mid-transition, before that prior step has actually finished closing out.
        private const float CutsceneStartDelay = 1.0f;

        private readonly Dictionary<string, Cutscene> cutscenesById = new();

        // The quest currently waiting on its Cutscene-type step to finish — guards against
        // re-triggering Play() on every OnQuestUpdated while it's already running.
        private Quest activeQuestStep;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Awake/OnEnable order between sibling scene objects isn't guaranteed — a
            // Cutscene already in the scene when this Awake runs may have already tried
            // (and silently no-op'd) to register itself before Instance existed here.
            foreach (var cutscene in FindObjectsByType<Cutscene>(FindObjectsSortMode.None))
                Register(cutscene);
        }

        private void OnEnable() => QuestManager.OnQuestUpdated += RefreshQuestStep;
        private void OnDisable() => QuestManager.OnQuestUpdated -= RefreshQuestStep;

        // QuestManager.InitializeQuests() runs in ITS Awake — a quest authored to start
        // already InProgress never fires OnQuestUpdated, so this OnEnable's subscription
        // alone could miss it. Start() is guaranteed to run after every object's
        // Awake/OnEnable — same fix already used for TutorialPromptUI/PlayerSelfDialogueUI.
        private void Start() => RefreshQuestStep();

        /// <summary>Plays the current Cutscene-type step's cutscene, if one is waiting and not already running.</summary>
        private void RefreshQuestStep()
        {
            if (QuestManager.Instance == null)
                return;

            foreach (var q in QuestManager.Instance.GetActiveQuests())
            {
                if (q.Status != QuestStatus.InProgress || q == activeQuestStep)
                    continue;

                var step = q.CurrentStep;
                if (step == null || step.Type != QuestType.Cutscene)
                    continue;

                var cutscene = Get(step.TargetEnemyId);
                if (cutscene == null)
                    continue; // authored id not in this scene yet — Register() retries this once it loads in

                activeQuestStep = q;
                StartCoroutine(PlayAfterDelay(q, step.TargetEnemyId, cutscene));
                return;
            }
        }

        private IEnumerator PlayAfterDelay(Quest q, string cutsceneId, Cutscene cutscene)
        {
            yield return new WaitForSeconds(CutsceneStartDelay);

            // Re-check rather than trust the snapshot — the quest could have moved on
            // (or been cancelled) during the wait.
            var step = q.CurrentStep;
            bool stillWaiting = q.Status == QuestStatus.InProgress &&
                step != null && step.Type == QuestType.Cutscene && step.TargetEnemyId == cutsceneId;

            if (!stillWaiting)
            {
                activeQuestStep = null;
                yield break;
            }

            cutscene.Play(() =>
            {
                activeQuestStep = null;
                CutsceneEvents.RaiseCutsceneFinished(cutsceneId);
            });
        }

        public void Register(Cutscene cutscene)
        {
            if (cutscene == null || string.IsNullOrEmpty(cutscene.CutsceneId))
                return;

            cutscenesById[cutscene.CutsceneId] = cutscene;

            // A Cutscene-type step may already be current and waiting for exactly this id
            // (e.g. this Cutscene's scene loaded in after the quest advanced to it).
            RefreshQuestStep();
        }

        public void Unregister(Cutscene cutscene)
        {
            if (cutscene == null || string.IsNullOrEmpty(cutscene.CutsceneId))
                return;

            // Only remove if this instance is still the one registered — avoids a late
            // OnDisable clobbering a newer registration for the same id.
            if (cutscenesById.TryGetValue(cutscene.CutsceneId, out var current) && current == cutscene)
                cutscenesById.Remove(cutscene.CutsceneId);
        }

        public Cutscene Get(string cutsceneId) =>
            !string.IsNullOrEmpty(cutsceneId) && cutscenesById.TryGetValue(cutsceneId, out var c) ? c : null;
    }
}
