using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class PlayerFeedbackVFX : MonoBehaviour
    {
        [Header("Damage VFX")]
        [SerializeField] private Image damageVignette;
        [SerializeField] private float damageFadeIn = 0.05f;
        [SerializeField] private float damageHoldTime = 0.1f;
        [SerializeField] private float damageFadeOut = 0.35f;
        [SerializeField, Range(0f, 1f)] private float damageAlpha = 0.65f;

        [Header("Sprint VFX")]
        [SerializeField] private Image sprintVignette;
        [SerializeField, Range(0f, 1f)] private float sprintAlpha = 0.15f;
        [SerializeField] private float sprintFadeSpeed = 5f;

        private Coroutine damageCoroutine;
        private Coroutine sprintCoroutine;

        private void Awake()
        {
            SetAlpha(damageVignette, 0f);
            SetAlpha(sprintVignette, 0f);
        }

        private void OnEnable() => Player.OnPlayerDamaged += PlayDamageVFX;
        private void OnDisable() => Player.OnPlayerDamaged -= PlayDamageVFX;

        // ==========================================
        // DAMAGE
        // ==========================================

        public void PlayDamageVFX()
        {
            if (damageVignette == null)
                return;

            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);

            damageCoroutine = StartCoroutine(DamageRoutine());
        }

        private IEnumerator DamageRoutine()
        {
            yield return FadeImage(
                damageVignette,
                damageAlpha,
                damageFadeIn
            );

            yield return new WaitForSeconds(damageHoldTime);

            yield return FadeImage(
                damageVignette,
                0f,
                damageFadeOut
            );

            damageCoroutine = null;
        }

        // ==========================================
        // SPRINT
        // ==========================================

        public void SetSprinting(bool isSprinting)
        {
            if (sprintVignette == null)
                return;

            if (sprintCoroutine != null)
                StopCoroutine(sprintCoroutine);

            sprintCoroutine = StartCoroutine(
                SprintRoutine(isSprinting)
            );
        }

        private IEnumerator SprintRoutine(bool active)
        {
            float targetAlpha = active ? sprintAlpha : 0f;

            while (Mathf.Abs(sprintVignette.color.a - targetAlpha) > 0.01f)
            {
                Color color = sprintVignette.color;

                color.a = Mathf.MoveTowards(
                    color.a,
                    targetAlpha,
                    sprintFadeSpeed * Time.deltaTime
                );

                sprintVignette.color = color;

                yield return null;
            }

            SetAlpha(sprintVignette, targetAlpha);

            sprintCoroutine = null;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private IEnumerator FadeImage(
            Image image,
            float targetAlpha,
            float duration
        )
        {
            float startAlpha = image.color.a;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);

                float alpha = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

                SetAlpha(image, alpha);

                yield return null;
            }

            SetAlpha(image, targetAlpha);
        }

        private void SetAlpha(Image image, float alpha)
        {
            if (image == null)
                return;

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}