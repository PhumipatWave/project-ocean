using System;

namespace Game.Core
{
    /// <summary>Raised whenever a Cutscene finishes playing. Payload = its cutsceneId.</summary>
    public static class CutsceneEvents
    {
        public static event Action<string> OnCutsceneFinished;

        public static void RaiseCutsceneFinished(string cutsceneId)
        {
            OnCutsceneFinished?.Invoke(cutsceneId);
        }
    }
}
