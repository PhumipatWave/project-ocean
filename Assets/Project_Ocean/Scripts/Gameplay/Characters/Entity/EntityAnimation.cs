using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Pure modular animation driver for any Entity.
    /// Has zero knowledge of specific parameter names or states.
    /// All callers (Enemy, Player, etc.) own their own parameter strings/hashes.
    /// </summary>
    public class EntityAnimation : MonoBehaviour
    {
        [SerializeField] private Animator anim;

        private void Awake()
        {
            if (anim == null)
                anim = GetComponentInChildren<Animator>();
        }

        /// <summary>Override the auto-discovered animator (called by ApplyLegacySerializedValues).</summary>
        public void Configure(Animator animator)
        {
            if (animator != null)
                anim = animator;

            if (anim == null)
                anim = GetComponentInChildren<Animator>();
        }

        public void PlayAnim(int hash)
        {
            if (anim == null) return;
            anim.SetTrigger(hash);
        }

        public void PlayAnim(int hash, int value)
        {
            if (anim == null) return;
            anim.SetInteger(hash, value);
        }

        public void PlayAnim(int hash, float value)
        {
            if (anim == null) return;
            anim.SetFloat(hash, value);
        }

        public void PlayAnim(int hash, bool value)
        {
            if (anim == null) return;
            anim.SetBool(hash, value);
        }
    }
}
