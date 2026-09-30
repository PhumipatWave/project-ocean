using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Game.Gameplay
{
    /// <summary>A Timeline track binding resolved at play time, since a prefab can't reference scene objects.</summary>
    // trackName must match the track name exactly. bindingKey is an NPC id, "Player" or "MainCamera".
    [Serializable]
    public class CutsceneTrackBinding
    {
        [Tooltip("Exact track name as shown in the Timeline window (e.g. \"Animation Track (1)\").")]
        public string trackName;
        [Tooltip("An NPC's Npc Id, optionally followed by a child path (e.g. \"hunter\" or \"hunter/Visual\"), or \"MainCamera\" for the main camera's binding (e.g. a Cinemachine Track's CinemachineBrain).")]
        public string bindingKey;
    }

    /// <summary>A Timeline cutscene played by CutsceneManager; locks player control while it runs.</summary>
    // Skip (Space) asks for confirmation first, then jumps to the end so the final frame's state is applied.
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

            // The player's Rigidbody2D would fight a Timeline track moving it; paused until HandleStopped.
            playerBody = PlayerRegistry.Local != null ? PlayerRegistry.Local.RB : null;
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.simulated = false;
            }

            var reader = PlayerRegistry.Local?.input;
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

        // "npcId/child/path" reaches a child of the NPC; "npcId/.." reaches its parent, which Transform.Find can't.
        private Component ResolveBindingTarget(string bindingKey)
        {
            if (string.Equals(bindingKey, "MainCamera", StringComparison.OrdinalIgnoreCase))
                return Camera.main;

            var slash = bindingKey.IndexOf('/');
            var npcId = slash < 0 ? bindingKey : bindingKey.Substring(0, slash);

            // "Player" is reserved: the player isn't registered with NPCManager.
            Component npc = string.Equals(npcId, "Player", StringComparison.OrdinalIgnoreCase)
                ? PlayerRegistry.Local
                : NPCManager.Instance?.GetNpc(npcId);
            if (npc == null || slash < 0)
                return npc;

            var childPath = bindingKey.Substring(slash + 1);

            if (childPath == "..")
                return npc.transform.parent;

            // NPC scripts sit on a child of a wrapper (Hunter/Talk), so try the path from the parent first.
            return npc.transform.Find(childPath)
                ?? (npc.transform.parent != null ? npc.transform.parent.Find(childPath) : null);
        }

        // ponytail: only track bindings are resolved, not per-clip ExposedReferences; add SetReferenceValue if needed.
        // ponytail: Skip is bound to Space in .inputactions but has no rebind slot, same as Teleport.
        private void HandleSkipPressed()
        {
            var prompt = ModalPrompt.Current;
            if (!isPlaying || confirmingSkip || prompt == null || !prompt.CanShow)
                return;

            confirmingSkip = true;
            prompt.Show("Skip this cutscene?", Skip, () => confirmingSkip = false);
        }

        private void Skip()
        {
            confirmingSkip = false;
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
            HandleStopped(director); // no-op if `stopped` already ran it
        }

        // director.stopped never fires if this is destroyed early, and the player lock would leak for the session.
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
                ModalPrompt.Current?.Cancel();

            CutsceneGate.IsPlaying = false;

            if (playerBody != null)
                playerBody.simulated = true;
            playerBody = null;

            var reader = PlayerRegistry.Local?.input;
            if (reader != null) reader.OnSkipCutscene -= HandleSkipPressed;

            PlayerUIState.Close();

            var callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }
    }
}
