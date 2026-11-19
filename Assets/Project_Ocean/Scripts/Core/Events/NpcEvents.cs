namespace Game.Core
{
    /// <summary>Raised whenever the player interacts with an NPC. Payload = that NPC's npcId.</summary>
    public static class NpcEvents
    {
        public static event System.Action<string> OnNpcTalkedTo;

        public static void RaiseNpcTalkedTo(string npcId)
        {
            OnNpcTalkedTo?.Invoke(npcId);
        }
    }
}
