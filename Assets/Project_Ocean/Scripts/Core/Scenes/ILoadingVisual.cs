namespace Game.Testing
{
    /// <summary>Loading-screen visual that LoadingManager (Core) drives without referencing UI types.</summary>
    public interface ILoadingVisual
    {
        void ApplyProfile(LoadingType type);
    }
}
