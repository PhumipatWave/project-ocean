using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Game.Testing
{
    /// <summary>
    /// One track's binding, resolved at play time rather than wired in the Inspector — this
    /// component lives on a PREFAB (see CutsceneManager), so it can't hold a direct scene
    /// reference the way a scene-placed Timeline normally would. Both fields are plain
    /// strings, typed in by hand — Unity's Project window doesn't expose a Timeline's
    /// individual tracks as draggable sub-assets, so a TrackAsset object-reference field
    /// (the earlier version of this class) had no way to actually be filled in through the
    /// Inspector. trackName just has to match a track's name exactly as shown in the Timeline
    /// window. bindingKey matches an NPC's Npc Id (resolved via NPCManager) or the reserved
    /// key "MainCamera" (resolved via Camera.main).
    /// </summary>
    [Serializable]
    public class CutsceneTrackBinding
    {
        [Tooltip("Exact track name as shown in the Timeline window (e.g. \"Animation Track (1)\").")]
        public string trackName;
        [Tooltip("An NPC's Npc Id, optionally followed by a child path (e.g. \"hunter\" or \"hunter/Visual\"), or \"MainCamera\" for the main camera's binding (e.g. a Cinemachine Track's CinemachineBrain).")]
        public string bindingKey;
    }

    /// <summary>
    /// A Timeline cutscene, instantiated on demand by CutsceneManager and destroyed when it
    /// finishes — see CutsceneManager for why (no permanent scene placement, one registry of
    /// prefabs instead). Triggered by id from a dialogue line (QuestDialogueLine.cutsceneId —
    /// see NPC.PlayCutsceneIfAny / PlayerSelfDialogueUI.Show) or a Cutscene-type quest step.
    ///
    /// Locks player control for the duration via PlayerUIState, same as any other blocking
    /// panel. The SkipCutscene input (Space) asks (ConfirmationDialog; the cutscene keeps playing meanwhile) whether to skip; Interact
    /// does nothing. Skipping jumps to the end rather than stopping abruptly, so the
    /// Timeline's final frame (camera/actor end state) is always fully applied.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class Cutscene : MonoBehaviour
    {
        [Tooltip("Matched by a QuestDialogueLine's Cutscene Id, or a Cutscene-type step's Cutscene Id.")]
        [SerializeField] private string cutsceneId;

        [Tooltip("Resolved against the current scene right before playing — this prefab has no scene references of its own.")]
        [SerializeField] private List<CutsceneTrackBinding> trackBindings = new();

        public string CutsceneId => cutsceneId;

        private PlayableDirector director;
        private Action onComplete;
        private bool isPlaying;
        private bool confirmingSkip;
        private Rigidbody2D playerBody;

        private void Awake() => director = GetComponent<PlayableDirector>();

        public void Play(Action onComplete = null)
        {
            if (isPlaying)
                return;

            isPlaying = true;
            this.onComplete = onComplete;
            CutsceneGate.IsPlaying = true;

            ResolveBindings();

            director.time = 0;
            director.Play();
            director.stopped += HandleStopped;

            PlayerUIState.Open();

            // The player's dynamic, interpolated Rigidbody2D writes the transform every physics
            // step — it would drag the root back against a Timeline track animating it
            // (PlayerPos). Take it out of the simulation for the cutscene; re-enabled in
            // HandleStopped, where it picks up wherever the Timeline left the player.
            playerBody = GameManager.CurPlayer != null ? GameManager.CurPlayer.RB : null;
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.simulated = false;
            }

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnSkipCutscene += HandleSkipPressed;
        }

        /// <summary>Also run <paramref name="callback"/> when this cutscene finishes — for a second
        /// request for the same cutscene while it is already playing (see CutsceneManager.Play).</summary>
        public void AddOnComplete(Action callback)
        {
            if (callback != null)
                onComplete += callback;
        }

        private void ResolveBindings()
        {
            var timeline = director.playableAsset as TimelineAsset;
            if (timeline == null || trackBindings.Count == 0)
                return;

            foreach (var binding in trackBindings)
            {
                if (string.IsNullOrEmpty(binding.trackName) || string.IsNullOrEmpty(binding.bindingKey))
                    continue;

                var track = timeline.GetOutputTracks().FirstOrDefault(t => t.name == binding.trackName);
                if (track == null)
                {
                    Debug.LogWarning($"[Cutscene] '{cutsceneId}': no track named '{binding.trackName}' in this Timeline.");
                    continue;
                }

                var targetType = track.outputs.FirstOrDefault().outputTargetType;
                if (targetType == null)
                {
                    Debug.LogWarning($"[Cutscene] '{cutsceneId}': track '{binding.trackName}' has no output binding to resolve.");
                    continue;
                }

                var target = ResolveBindingTarget(binding.bindingKey);
                if (target == null)
                {
                    Debug.LogWarning($"[Cutscene] '{cutsceneId}' couldn't resolve binding key '{binding.bindingKey}' — not found in the current scene.");
                    continue;
                }

                UnityEngine.Object resolved = targetType == typeof(GameObject)
                    ? target.gameObject
                    : target.GetComponent(targetType);

                if (resolved == null)
                {
                    Debug.LogWarning($"[Cutscene] '{cutsceneId}': '{binding.bindingKey}' has no {targetType.Name} for track '{binding.trackName}'.");
                    continue;
                }

                director.SetGenericBinding(track, resolved);
            }
        }

        // bindingKey is an NPC id, optionally followed by "/childPath" to reach a child under
        // it (e.g. "hunter/Visual") — NPCManager only ever knows about an NPC's own root, not
        // its children, so a track that needs a component on a specific child (a sprite/visual
        // object, a separate Animator, etc.) walks down from there via Transform.Find. The
        // special path ".." reaches the NPC's own PARENT instead (e.g. "hunter/.." for the
        // Hunter wrapper GameObject, as opposed to "hunter" which resolves to the NPC script's
        // own GameObject, Talk) — Transform.Find can't return a parent, so that case is handled
        // directly rather than going through it.
        private Component ResolveBindingTarget(string bindingKey)
        {
            if (string.Equals(bindingKey, "MainCamera", StringComparison.OrdinalIgnoreCase))
                return Camera.main;

            var slash = bindingKey.IndexOf('/');
            var npcId = slash < 0 ? bindingKey : bindingKey.Substring(0, slash);

            // "Player" is reserved like "MainCamera" — the player isn't an NPC, so NPCManager
            // never knows about it. Its root has no wrapper parent, so child paths
            // (e.g. "player/Visual") resolve directly under it via the Find below.
            Component npc = string.Equals(npcId, "Player", StringComparison.OrdinalIgnoreCase)
                ? GameManager.CurPlayer
                : NPCManager.Instance?.GetNpc(npcId);
            if (npc == null || slash < 0)
                return npc;

            var childPath = bindingKey.Substring(slash + 1);

            if (childPath == "..")
                return npc.transform.parent;

            // This project's NPCs live on a child of a wrapper GameObject (e.g. Hunter/Talk),
            // with sibling objects (Visual, Canvas) holding the actual rendering — so a child
            // path usually resolves relative to the NPC's PARENT, not its own transform. Try
            // both so this still works for an NPC that isn't wrapped this way.
            return npc.transform.Find(childPath)
                ?? (npc.transform.parent != null ? npc.transform.parent.Find(childPath) : null);
        }

        // ponytail: per-clip ExposedReferences (e.g. swapping which Cinemachine camera a
        // specific CinemachineShot clip points at) aren't resolved here — only track-level
        // bindings. Add PlayableDirector.SetReferenceValue resolution if a cutscene needs that.
        // ponytail: bound to Space in the .inputactions but not in KeyAction, so no rebind UI slot — same as Teleport.
        private void HandleSkipPressed()
        {
            if (!isPlaying || confirmingSkip || ConfirmationDialog.Instance == null ||
                (UIManager.Instance != null && UIManager.Instance.IsUIOpen))
                return;

            confirmingSkip = true;
            ConfirmationDialog.Instance.Show("Skip this cutscene?", Skip, () => confirmingSkip = false);
        }

        private void Skip()
        {
            confirmingSkip = false;
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
            HandleStopped(director); // no-op if `stopped` already ran it
        }

        // Safety net: HandleStopped normally runs off director.stopped, but that event never
        // fires if this instance is destroyed abnormally (Play mode stopped mid-cutscene, scene
        // torn down, etc.) — without this, PlayerUIState's Open() from Play() above never gets
        // its matching Close(), leaking the lock for the rest of the session and permanently
        // preventing GameHUD's hide-on-cutscene (its 0→1 transition never re-fires).
        private void OnDestroy()
        {
            if (isPlaying)
                HandleStopped(director);
        }

        private void HandleStopped(PlayableDirector d)
        {
            if (!isPlaying)
                return;

            director.stopped -= HandleStopped;
            isPlaying = false;

            // Cutscene ended on its own while the skip dialog was still open — close it (Cancel
            // runs the callback above, resetting confirmingSkip) instead of leaving it over nothing.
            if (confirmingSkip)
                ConfirmationDialog.Instance?.Cancel();

            CutsceneGate.IsPlaying = false;

            if (playerBody != null)
                playerBody.simulated = true;
            playerBody = null;

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnSkipCutscene -= HandleSkipPressed;

            PlayerUIState.Close();

            var callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }
    }
}
