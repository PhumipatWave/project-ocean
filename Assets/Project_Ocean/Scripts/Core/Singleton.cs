using UnityEngine;

namespace Game.Core
{
    /// <summary>First-wins MonoBehaviour singleton: later copies destroy themselves and <see cref="Instance"/> clears on destroy.</summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        /// <summary>True on a copy that lost to an existing Instance; overrides of Awake must return early.</summary>
        protected bool IsDuplicate { get; private set; }

        // Survives scene loads. Only a root can be DontDestroyOnLoad; children ride on their root's.
        protected virtual bool Persistent => false;

        // False when the component shares its GameObject with other components that must survive.
        protected virtual bool DestroyGameObjectOnDuplicate => true;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                IsDuplicate = true;
                if (DestroyGameObjectOnDuplicate) Destroy(gameObject);
                else Destroy(this);
                return;
            }

            Instance = (T)this;
            if (Persistent && transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        // Overrides do their own cleanup first, then call base (it clears Instance).
        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
