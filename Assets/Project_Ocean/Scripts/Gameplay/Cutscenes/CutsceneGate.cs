using System;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>True while a cutscene plays, so the press that skips it doesn't also advance a dialogue.</summary>
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
