namespace Game.Testing
{
    /// <summary>The quest accept/reject panel an NPC opens after its offer dialogue.</summary>
    public interface IQuestOfferView
    {
        void ShowPanel(Quest quest, System.Action onStatusChanged);
        void HidePanel();
    }
}
