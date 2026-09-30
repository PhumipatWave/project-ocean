namespace Game.Core
{
    /// <summary>Screen area a notification shows in.</summary>
    // Serialized as int on views in AS_PlayerHUD — only append new values.
    public enum NotificationChannel
    {
        General,  // top center
        Center,   // quest + tutorial
        System,   // bottom right
        Resource  // bottom left, money + items
    }
}
