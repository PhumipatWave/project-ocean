using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Plays cutscene prefabs by id and runs Cutscene-type quest steps; each instance is destroyed when it ends.</summary>
    // Cutscenes are rare (one per quest beat), so instantiate-and-destroy is cheap enough; no pool needed.
    public class CutsceneManager : Singleton<CutsceneManager>
    {

        [Tooltip("Every cutscene prefab in the project — each needs a Cutscene component with a unique Cutscene Id.")]
        [SerializeField] private List<GameObject> cutscenePrefabs = new();

        // The step becomes current while the previous step is still closing (e.g. mid NPC dialogue close).
        private const float CutsceneStartDelay = 1.0f;

        private readonly Dictionary<string, GameObject> prefabsById = new();

        // Cutscenes playing right now, by id — a second request for the same id joins the running
        // one instead of stacking a duplicate on top of it (see Play).
        private readonly Dictionary<string, Cutscene> running = new();

        // The quest currently waiting on its Cutscene-type step to finish — guards against
        // re-triggering Play() on every OnQuestUpdated while it's already running.
        private Quest activeQuestStep;

        // Lock the player during the delay too, or they walk off and the cutscene starts from wherever they went.
        private bool holdingStartLock;

        // A scene load during the delay resets PlayerUIState; releasing after that would drop the cutscene's own lock.
        private int startLockResetCount;

        protected override bool Persistent => true;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

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
            LoadingManager.OnLoadingFinished += HandleLoadingFinished;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            QuestManager.OnQuestUpdated -= RefreshQuestStep;
            LoadingManager.OnLoadingFinished -= HandleLoadingFinished;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            ReleaseStartLock();

            // If disabled mid-delay, the coroutine never clears this, and the step would be skipped forever.
            activeQuestStep = null;
        }

        private void ReleaseStartLock()
        {
            if (!holdingStartLock) return;
            holdingStartLock = false;
            if (PlayerUIState.ResetCount == startLockResetCount)
                PlayerUIState.Close();
        }

        /// <summary>Cancels any pending or running cutscene for a New Game; scene loads never disable this persistent manager.</summary>
        public void ResetToDefault()
        {
            StopAllCoroutines();
            ReleaseStartLock();
            activeQuestStep = null;

            // Each cutscene's OnDestroy cleans up its own lock; the quests its callbacks reference were already replaced.
            foreach (var cutscene in new List<Cutscene>(running.Values))
                if (cutscene != null)
                    Destroy(cutscene.gameObject);
            running.Clear();
        }

        // Quests start in QuestManager.Awake without an update event, so check once after every Awake.
        private void Start() => RefreshQuestStep();

        /// <summary>Plays the current Cutscene-type step's cutscene, if one is waiting and not already running.</summary>
        // Scene loads that skip LoadingManager (Continue/SaveBridge) still get a re-check here.
        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
            => RefreshQuestStep();

        // No step is closing right after a load, so skip the delay; otherwise gameplay shows for a second before the cutscene.
        private void HandleLoadingFinished() => RefreshQuestStep(delayStart: false);

        private void RefreshQuestStep() => RefreshQuestStep(delayStart: true);

        private void RefreshQuestStep(bool delayStart)
        {
            if (QuestManager.Instance == null)
                return;

            // Bootstrap/Intro/menu would play it unseen and count it as done; wait for a gameplay scene.
            if (LoadingManager.IsLoading ||
                !SceneNames.IsGameplay(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                return;

            foreach (var q in QuestManager.Instance.GetActiveQuests())
            {
                if (q.Status != QuestStatus.InProgress || q == activeQuestStep)
                    continue;

                var step = q.CurrentStep;
                // Auto Display Dialogue steps are played by PlayerSelfDialogueUI instead.
                if (step == null || step.AutoDisplayDialogue ||
                    step.EffectiveCutsceneId == null || !prefabsById.ContainsKey(step.EffectiveCutsceneId))
                    continue;

                activeQuestStep = q;
                PlayerUIState.Open();
                holdingStartLock = true;
                startLockResetCount = PlayerUIState.ResetCount;
                StartCoroutine(PlayStepAfterDelay(q, step.EffectiveCutsceneId, delayStart ? CutsceneStartDelay : 0f));
                return;
            }
        }

        private IEnumerator PlayStepAfterDelay(Quest q, string cutsceneId, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

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

        /// <summary>Plays the cutscene prefab with this id; false if none is registered.</summary>
        public bool Play(string cutsceneId, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(cutsceneId) || !prefabsById.TryGetValue(cutsceneId, out var prefab))
                return false;

            // A dialogue line and a Cutscene step can request the same id a second apart; join the running copy
            // instead of overlapping, since a skip only ends one of them.
            if (running.TryGetValue(cutsceneId, out var already) && already != null)
            {
                already.AddOnComplete(onComplete);
                return true;
            }

            var instance = Instantiate(prefab);

            // A prefab saved disabled never runs Cutscene.Awake, so Play would throw; activate it.
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
