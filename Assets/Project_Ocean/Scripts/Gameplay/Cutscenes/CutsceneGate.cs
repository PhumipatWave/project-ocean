namespace Game.Testing
{
    /// <summary>
    /// Set while any Cutscene is playing (see Cutscene.Play/HandleStopped) — other systems
    /// that react to the raw Interact press (InteractUI's NPC dialogue advance,
    /// PlayerSelfDialogueUI's dismiss) check this first, so skipping a cutscene doesn't also
    /// advance/close whatever dialogue triggered it in the same press.
    /// </summary>
    public static class CutsceneGate
    {
        public static bool IsPlaying;
    }
}
