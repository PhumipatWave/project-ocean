using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Relays SaveBridge.OnSaved into the generic NotificationManager, so the
    /// "Game Saved" message reuses the shared NotificationTextUI toast instead
    /// of rendering its own fade. Put on any always-loaded GameObject (e.g. GameHUD).
    /// </summary>
    public class SaveNotificationUI : MonoBehaviour
    {
        [SerializeField] private string message = "Game Saved!";

        private void OnEnable()  => SaveBridge.OnSaved += HandleSaved;
        private void OnDisable() => SaveBridge.OnSaved -= HandleSaved;

        private void HandleSaved() => NotificationManager.Notify(message);
    }
}
