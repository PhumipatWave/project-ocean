using UnityEngine;

namespace Game.Testing
{
    public class EnemyAnimation : MonoBehaviour
    {
        #region FieldsProperties
        [SerializeField] private bool hasAnimator;
        [SerializeField] private Animator anim;
        #endregion

        #region UnityCallbacks
        private void Awake()
        {
            if (hasAnimator && anim == null)
                anim = GetComponentInChildren<Animator>();
        }
        #endregion

        public void Configure(bool enabled, Animator animator)
        {
            hasAnimator = enabled;

            if (animator != null)
                anim = animator;

            if (hasAnimator && anim == null)
                anim = GetComponentInChildren<Animator>();
        }

        public void SetAttacking(bool isAttacking)
        {
            if (!hasAnimator || anim == null)
                return;

            anim.SetBool("isAttack_01", isAttacking);
        }

        public void PlayTakeDamage()
        {
            if (!hasAnimator || anim == null)
                return;

            anim.SetTrigger("takeDamage");
        }

        public void PlayDeath()
        {
            if (!hasAnimator || anim == null)
                return;

            anim.SetBool("isDeath", true);
        }
    }
}
