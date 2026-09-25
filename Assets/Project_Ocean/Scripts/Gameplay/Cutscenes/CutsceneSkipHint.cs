using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shows the skip hint on the System channel for the whole cutscene.</summary>
    public class CutsceneSkipHint : MonoBehaviour
    {
        [SerializeField] private string message = "Press [Space] to skip cutscene";

        private void OnEnable()
        {
            CutsceneGate.OnPlayingChanged += HandlePlayingChanged;
            if (CutsceneGate.IsPlaying) HandlePlayingChanged(true);
        }

        private void OnDisable() => CutsceneGate.OnPlayingChanged -= HandlePlayingChanged;

        private void HandlePlayingChanged(bool playing)
        {
            if (playing) NotificationManager.Notify(message, NotificationChannel.System, null, sticky: true);
            else NotificationManager.Clear(NotificationChannel.System);
        }
    }
}
