using System;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Set while any Cutscene is playing (see Cutscene.Play/HandleStopped) — other systems
    /// that react to the raw Interact press (InteractUI's NPC dialogue advance,
    /// PlayerSelfDialogueUI's dismiss) check this first, so skipping a cutscene doesn't also
    /// advance/close whatever dialogue triggered it in the same press.
    /// </summary>
    public static class CutsceneGate
    {
        private static bool isPlaying;

        public static event Action<bool> OnPlayingChanged;

        public static bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (isPlaying == value) return;
                isPlaying = value;
                OnPlayingChanged?.Invoke(value);
            }
        }
    }
}
