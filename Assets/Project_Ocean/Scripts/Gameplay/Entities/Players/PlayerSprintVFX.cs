using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class PlayerSprintVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem sprintEffect;

        // Authored rotation = emitter pointing for rightward movement.
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalPosition;

        private void Awake()
        {
            if (sprintEffect == null)
                sprintEffect = GetComponent<ParticleSystem>();

            baseLocalRotation = transform.localRotation;
            baseLocalPosition = transform.localPosition;
            StopEffect();
        }

        // Aligns emitter and line sprites with the move direction (world space).
        public void SetDirection(Vector2 worldDir)
        {
            if (sprintEffect == null || worldDir == Vector2.zero)
                return;

            // Parent (Visual) is Y-flipped when facing left, so work in its local space for the emitter.
            Vector3 localDir = transform.parent.InverseTransformDirection(worldDir);
            float localAngle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
            Quaternion turn = Quaternion.Euler(0f, 0f, localAngle);
            transform.localRotation = turn * baseLocalRotation;

            // Keep feet height; swing only the "behind" offset so the trail stays behind the move direction.
            transform.localPosition = new Vector3(0f, baseLocalPosition.y, baseLocalPosition.z)
                + turn * new Vector3(baseLocalPosition.x, 0f, 0f);

            // Billboards ignore transform rotation; particle rotation is clockwise, hence the minus.
            var main = sprintEffect.main;
            main.startRotation = -Mathf.Atan2(worldDir.y, worldDir.x);
        }

        public void SetSprinting(bool isSprinting)
        {
            if (sprintEffect == null)
                return;

            if (isSprinting)
            {
                if (!sprintEffect.isPlaying)
                    sprintEffect.Play();
            }
            else
            {
                StopEffect();
            }
        }

        private void StopEffect()
        {
            sprintEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}
