using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shared handshake for "we are loading from a save file" that must survive a
    /// scene load. Owns the contract that was previously split across SaveBridge's
    /// statics and a comment in GameManager.
    ///
    /// Sequence (do not reorder):
    ///   1. SaveBridge.LoadGame sets Position then BeginLoad() BEFORE LoadScene().
    ///   2. On sceneLoaded, GameManager reads IsLoading + Position to place the player,
    ///      and intentionally does NOT clear the flag.
    ///   3. SaveBridge's post-load coroutine applies stats/inventory/etc, then calls
    ///      Clear() — exactly once, after everything is applied.
    ///
    /// Neither GameManager nor SaveBridge references the other for this handshake;
    /// both reference only this context, breaking the circular type dependency.
    /// </summary>
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
