namespace Game.Core
{
    /// <summary>
    /// Implemented by any system that owns a slice of the save file.
    /// SaveBridge holds an explicitly ordered list of participants and drives
    /// Collect (on save) and Apply (on load) in that fixed order — deterministic,
    /// no self-registration, so the timing-sensitive load sequence stays predictable.
    /// </summary>
    public interface ISaveParticipant
    {
        /// <summary>Write this system's state into the save data.</summary>
        void Collect(SaveData data);

        /// <summary>Restore this system's state from the save data.</summary>
        void Apply(SaveData data);
    }
}
