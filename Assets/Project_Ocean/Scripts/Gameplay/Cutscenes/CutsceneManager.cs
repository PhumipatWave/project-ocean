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
                if (step == null || step.EffectiveCutsceneId == null || !prefabsById.ContainsKey(step.EffectiveCutsceneId))
                    continue;

                activeQuestStep = q;
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
                yield break;
            }

            Play(cutsceneId, () =>
            {
                activeQuestStep = null;
                CutsceneEvents.RaiseCutsceneFinished(cutsceneId);
            });
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

            var instance = Instantiate(prefab);

            // Unity never runs Awake on an inactive GameObject (or its instantiated clone)
            // until it's activated — if the prefab was ever accidentally saved disabled,
            // Cutscene's own Awake (which fetches its PlayableDirector) would silently never
            // run, and Play() below would NRE. Cheap to guard against a plausible authoring mistake.
            if (!instance.activeSelf)
                instance.SetActive(true);

            instance.GetComponent<Cutscene>().Play(() =>
            {
                Destroy(instance);
                onComplete?.Invoke();
            });
            return true;
        }
    }
}
