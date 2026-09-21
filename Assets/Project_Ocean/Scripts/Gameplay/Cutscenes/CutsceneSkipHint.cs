using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shows the "Press Space to skip" text on this object only while a cutscene plays.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CutsceneSkipHint : MonoBehaviour
    {
        private CanvasGroup group;

        private void Awake() => group = GetComponent<CanvasGroup>();

        private void OnEnable()
        {
            CutsceneGate.OnPlayingChanged += SetVisible;
            SetVisible(CutsceneGate.IsPlaying);
        }

        private void OnDisable() => CutsceneGate.OnPlayingChanged -= SetVisible;

        private void SetVisible(bool visible) => group.alpha = visible ? 1f : 0f;
    }
}
