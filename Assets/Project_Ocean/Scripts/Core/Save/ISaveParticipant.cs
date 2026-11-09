namespace Game.Core
{
    /// <summary>A system that owns a slice of the save file; SaveBridge calls them in a fixed order.</summary>
    public interface ISaveParticipant
    {
        /// <summary>Write this system's state into the save data.</summary>
        void Collect(SaveData data);

        /// <summary>Restore this system's state from the save data.</summary>
        void Apply(SaveData data);
    }
}
