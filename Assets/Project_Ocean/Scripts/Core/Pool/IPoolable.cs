namespace Game.Core
{
    /// Implement on any MonoBehaviour component that needs to reset state
    /// when taken from or returned to the ObjectPoolManager.
    public interface IPoolable
    {
        /// Called just after the object is activated and positioned by the pool.
        void OnGetFromPool();

        /// Called just before the object is deactivated and queued back into the pool.
        void OnReturnToPool();
    }
}
