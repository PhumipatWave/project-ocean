using System;
using Game.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace Game.Testing
{
    /// <summary>
    /// A Timeline cutscene placed in the scene, triggered by id from a dialogue line
    /// (QuestDialogueLine.cutsceneId — see NPC.PlayCutsceneIfAny / PlayerSelfDialogueUI.Show).
    /// Bindings (which Animator, which Cinemachine camera) are wired on this GameObject's own
    /// PlayableDirector in the Inspector, same as any hand-authored Timeline — the id indirection
    /// only exists because QuestData can't hold a direct scene-object reference.
    ///
    /// Locks player control for the duration via PlayerUIState, same as any other blocking
    /// panel. One Interact press skips to the end rather than stopping abruptly, so the
    /// Timeline's final frame (camera/actor end state) is always fully applied.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class Cutscene : MonoBehaviour
    {
        [Tooltip("Matched by a QuestDialogueLine's Cutscene Id.")]
        [SerializeField] private string cutsceneId;

        public string CutsceneId => cutsceneId;

        private PlayableDirector director;
        private Action onComplete;
        private bool isPlaying;

        private void Awake() => director = GetComponent<PlayableDirector>();

        private void OnEnable() => CutsceneManager.Instance?.Register(this);
        private void OnDisable() => CutsceneManager.Instance?.Unregister(this);

        public void Play(Action onComplete = null)
        {
            if (isPlaying)
                return;

            isPlaying = true;
            this.onComplete = onComplete;
            CutsceneGate.IsPlaying = true;

            director.time = 0;
            director.Play();
            director.stopped += HandleStopped;

            PlayerUIState.Open();

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted += HandleInteract;
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed || !isPlaying)
                return;

            director.time = director.duration;
            director.Evaluate();
            director.Stop();
        }

        private void HandleStopped(PlayableDirector d)
        {
            director.stopped -= HandleStopped;
            isPlaying = false;
            CutsceneGate.IsPlaying = false;

            var reader = GameManager.CurPlayer?.input;
            if (reader != null) reader.OnInteracted -= HandleInteract;

            PlayerUIState.Close();

            var callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }
    }
}
