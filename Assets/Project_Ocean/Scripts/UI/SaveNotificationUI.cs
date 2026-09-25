using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shows "Game Saved!" on the System channel after every save.</summary>
    public class SaveNotificationUI : MonoBehaviour
    {
        [SerializeField] private string message = "Game Saved!";

        private void OnEnable()  => SaveBridge.OnSaved += HandleSaved;
        private void OnDisable() => SaveBridge.OnSaved -= HandleSaved;

        private void HandleSaved() => NotificationManager.Notify(message, NotificationChannel.System);
    }
}
