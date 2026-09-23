using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Every cutscene in the project is a prefab (Cutscene + PlayableDirector), assigned once
    /// here rather than hand-placed in every scene that needs one. Playing an id instantiates
    /// the matching prefab, resolves its bindings against whatever's actually in the current
    /// scene (see Cutscene.ResolveBindings/NPCManager), and destroys the instance the moment
    /// it finishes — a cutscene plays rarely enough (once per quest beat) that this is plenty
    /// cheap; a real object pool would only add bookkeeping with no payoff at that frequency.
    ///
    /// Also drives QuestType.Cutscene steps directly: unlike a cutscene attached to one
    /// Talk line (triggered by NPC.cs/PlayerSelfDialogueUI when that line shows), a
    /// Cutscene-type step has no dialogue to hang off of — it plays the moment it becomes
    /// the current step and completes itself via CutsceneEvents when done.
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        [Tooltip("Every cutscene prefab in the project — each needs a Cutscene component with a unique Cutscene Id.")]
        [SerializeField] private List<GameObject> cutscenePrefabs = new();

        // A Cutscene-type step becomes current the instant the PREVIOUS step's completion
        // event fires — e.g. NPC.AdvanceDialogue() raises NpcEvents.RaiseNpcTalkedTo before
        // it even calls CloseDialogue(). Without this delay the cutscene would snatch control
        // mid-transition, before that prior step has actually finished closing out.
        private const float CutsceneStartDelay = 1.0f;

        private readonly Dictionary<string, GameObject> prefabsById = new();

        // Cutscenes playing right now, by id — a second request for the same id joins the running
        // one instead of stacking a duplicate on top of it (see Play).
        private readonly Dictionary<string, Cutscene> running = new();

        // The quest currently waiting on its Cutscene-type step to finish — guards against
        // re-triggering Play() on every OnQuestUpdated while it's already running.
        private Quest activeQuestStep;

        // The player is locked for the whole CutsceneStartDelay, not just once the cutscene
        // itself starts — otherwise they can keep walking (or open a panel) for that second and
        // the cutscene begins from wherever they wandered to. Released once Cutscene.Play has
        // taken its own lock, or if the wait is abandoned.
        private bool holdingStartLock;

        // PlayerUIState.ResetCount when the start lock was taken. A scene load during
        // CutsceneStartDelay (New Game → Land_Map) resets PlayerUIState; releasing after that
        // would close the cutscene's OWN lock instead and show the HUD mid-cutscene.
        private int startLockResetCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var prefab in cutscenePrefabs)
            {
                if (prefab == null)
                    continue;

                var cutscene = prefab.GetComponent<Cutscene>();
                if (cutscene == null || string.IsNullOrEmpty(cutscene.CutsceneId))
                {
                    Debug.LogWarning($"[CutsceneManager] Prefab '{prefab.name}' has no Cutscene component or Cutscene Id — skipped.");
                    continue;
                }

                prefabsById[cutscene.CutsceneId] = prefab;
            }
        }

        private void OnEnable()
        {
            QuestManager.OnQuestUpdated += RefreshQuestStep;
            LoadingManager.OnLoadingFinished += RefreshQuestStep;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= RefreshQuestStep;
            LoadingManager.OnLoadingFinished -= RefreshQuestStep;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            ReleaseStartLock();

            // Safety net: if this gets disabled while PlayStepAfterDelay is still waiting out
            // CutsceneStartDelay (Play mode stopped mid-wait, etc.), the coroutine dies without
            // ever reaching its own activeQuestStep = null reset — leaving that quest's Cutscene
            // step permanently stuck (RefreshQuestStep's q == activeQuestStep guard silently
            // skips it forever after, even across a fresh Play session with no domain reload).
            activeQuestStep = null;
        }

        private void ReleaseStartLock()
        {
            if (!holdingStartLock) return;
            holdingStartLock = false;
            if (PlayerUIState.ResetCount == startLockResetCount)
                PlayerUIState.Close();
        }

        /// <summary>Cancels any in-flight cutscene state — called by SaveBridge.ResetForNewGame().
        /// This component is DontDestroyOnLoad, so a normal scene load (e.g. returning to Main
        /// Menu) never disables it — interrupting a cutscene mid-CutsceneStartDelay or mid-play
        /// that way leaves its coroutine/lock/activeQuestStep running, and it can fire late (or
        /// block the guard in RefreshQuestStep) on the next "New Game" attempt, without this.</summary>
        public void ResetToDefault()
        {
            StopAllCoroutines();
            ReleaseStartLock();
            activeQuestStep = null;

            // OnDestroy's safety net (see Cutscene.cs) cleans up PlayerUIState/onComplete for
            // each one — harmless here since QuestManager.ResetToDefault() (called just before
            // this) already replaced every Quest object those callbacks could act on.
            foreach (var cutscene in new List<Cutscene>(running.Values))
                if (cutscene != null)
                    Destroy(cutscene.gameObject);
            running.Clear();
        }

        // QuestManager.InitializeQuests() runs in ITS Awake — a quest authored to start
        // already InProgress never fires OnQuestUpdated, so this OnEnable's subscription
        // alone could miss it. Start() is guaranteed to run after every object's
        // Awake/OnEnable — same fix already used for TutorialPromptUI/PlayerSelfDialogueUI.
        private void Start() => RefreshQuestStep();

        /// <summary>Plays the current Cutscene-type step's cutscene, if one is waiting and not already running.</summary>
        // Scene loads that skip LoadingManager (Continue/SaveBridge) still get a re-check here.
        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
            => RefreshQuestStep();

        private void RefreshQuestStep()
        {
            if (QuestManager.Instance == null)
                return;

            // This manager lives from the Main Menu on (AS_PlayerHUD loads with it). A quest
            // cutscene started there plays unseen and gets destroyed by the Land load, which
            // marks its step done. Wait for gameplay; OnLoadingFinished/sceneLoaded re-check.
            if (LoadingManager.IsLoading ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneNames.MainMenu)
                return;

            foreach (var q in QuestManager.Instance.GetActiveQuests())
            {
                if (q.Status != QuestStatus.InProgress || q == activeQuestStep)
                    continue;

                var step = q.CurrentStep;
                // Auto Display Dialogue steps are driven by PlayerSelfDialogueUI instead —
                // press-to-advance dialogue, not an auto-played Timeline. See Quest.BuildSteps
                // and PlayerSelfDialogueUI's class doc.
                if (step == null || step.AutoDisplayDialogue ||
                    step.EffectiveCutsceneId == null || !prefabsById.ContainsKey(step.EffectiveCutsceneId))
                    continue;

                activeQuestStep = q;
                PlayerUIState.Open();
                holdingStartLock = true;
                startLockResetCount = PlayerUIState.ResetCount;
                StartCoroutine(PlayStepAfterDelay(q, step.EffectiveCutsceneId));
                return;
            }
        }

        private IEnumerator PlayStepAfterDelay(Quest q, string cutsceneId)
        {
            yield return new WaitForSeconds(CutsceneStartDelay);

            // Re-check rather than trust the snapshot — the quest could have moved on
            // (or been cancelled) during the wait.
            var step = q.CurrentStep;
            bool stillWaiting = q.Status == QuestStatus.InProgress &&
                step != null && step.EffectiveCutsceneId == cutsceneId;

            if (!stillWaiting)
            {
                activeQuestStep = null;
                ReleaseStartLock();
                yield break;
            }

            Play(cutsceneId, () =>
            {
                activeQuestStep = null;
                CutsceneEvents.RaiseCutsceneFinished(cutsceneId);
            });

            // After Play, so the cutscene's own lock is already held — no unlocked frame in between.
            ReleaseStartLock();
        }

        /// <summary>
        /// Instantiates the cutscene prefab matching this id (into the current active scene),
        /// plays it, and destroys the instance once it finishes or is skipped. Returns false
        /// if no prefab is registered for this id.
        /// </summary>
        public bool Play(string cutsceneId, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(cutsceneId) || !prefabsById.TryGetValue(cutsceneId, out var prefab))
                return false;

            // The same id can be asked for twice at once: a dialogue line's Cutscene Id plays it the
            // moment the dialogue closes, and a Cutscene-type step with that id starts it again a
            // second later (CutsceneStartDelay) — while the first is still running. Two copies
            // overlap, and a skip only ever ends one of them. Join the running one instead: its
            // completion also completes this request (e.g. the quest's Cutscene step).
            if (running.TryGetValue(cutsceneId, out var already) && already != null)
            {
                already.AddOnComplete(onComplete);
                return true;
            }

            var instance = Instantiate(prefab);

            // Unity never runs Awake on an inactive GameObject (or its instantiated clone)
            // until it's activated — if the prefab was ever accidentally saved disabled,
            // Cutscene's own Awake (which fetches its PlayableDirector) would silently never
            // run, and Play() below would NRE. Cheap to guard against a plausible authoring mistake.
            if (!instance.activeSelf)
                instance.SetActive(true);

            var cutscene = instance.GetComponent<Cutscene>();
            running[cutsceneId] = cutscene;
            cutscene.Play(() =>
            {
                running.Remove(cutsceneId);
                Destroy(instance);
                onComplete?.Invoke();
            });
            return true;
        }
    }
}
