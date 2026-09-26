using UnityEngine;

namespace Game.Testing
{
    public class EntityAnimation : MonoBehaviour
    {
        [SerializeField] private Animator anim;

        private readonly System.Collections.Generic.HashSet<int> parameterHashes = new System.Collections.Generic.HashSet<int>();
        private RuntimeAnimatorController cachedController;

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

        // Enemy controllers don't share one parameter set (jellyfish has none, crab uses isAttack_01),
        // so skip missing ones instead of letting Animator log an error every frame.
        private bool HasParameter(int hash)
        {
            if (anim == null || !anim.isInitialized) return false;
            if (anim.runtimeAnimatorController != cachedController)
            {
                cachedController = anim.runtimeAnimatorController;
                parameterHashes.Clear();
                foreach (var p in anim.parameters)
                    parameterHashes.Add(p.nameHash);
            }
            return parameterHashes.Contains(hash);
        }

        public void PlayAnim(int hash)
        {
            if (!HasParameter(hash)) return;
            anim.SetTrigger(hash);
        }

        public void PlayAnim(int hash, int value)
        {
            if (!HasParameter(hash)) return;
            anim.SetInteger(hash, value);
        }

        public void PlayAnim(int hash, float value)
        {
            if (!HasParameter(hash)) return;
            anim.SetFloat(hash, value);
        }

        public void PlayAnim(int hash, bool value)
        {
            if (!HasParameter(hash)) return;
            anim.SetBool(hash, value);
        }
    }
}
