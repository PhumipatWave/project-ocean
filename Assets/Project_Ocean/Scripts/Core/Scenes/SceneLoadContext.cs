using UnityEngine;

namespace Game.Core
{
    /// <summary>Save-load handoff across a scene load, so GameManager and SaveBridge never reference each other.</summary>
    // Order: SaveBridge sets Position + BeginLoad before LoadScene; scene-load code places the player;
    // SaveBridge calls Clear once, after all save data is applied.
    public static class SceneLoadContext
    {
        /// <summary>True between BeginLoad() and Clear() — i.e. a save-load is in flight.</summary>
        public static bool IsLoading { get; private set; }

        /// <summary>Player position to restore, captured before the scene load.</summary>
        public static Vector3 Position { get; private set; }

        /// <summary>Called by SaveBridge before LoadScene(). Position must be set first.</summary>
        public static void BeginLoad(Vector3 position)
        {
            Position  = position;
            IsLoading = true;
        }

        /// <summary>Called once by SaveBridge after all post-load data has been applied.</summary>
        public static void Clear() => IsLoading = false;
    }
}
