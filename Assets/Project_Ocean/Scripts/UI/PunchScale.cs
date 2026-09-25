using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Briefly scales this object up and back to draw the eye.</summary>
    public class PunchScale : MonoBehaviour
    {
        [SerializeField] private float peakScale = 1.2f;
        [SerializeField] private float duration = 0.25f;

        private Vector3 baseScale;
        private Coroutine routine;

        private void Awake() => baseScale = transform.localScale;

        private void OnDisable()
        {
            // Hidden mid-pop would otherwise leave it stuck enlarged.
            routine = null;
            transform.localScale = baseScale;
        }

        public void Play()
        {
            if (!isActiveAndEnabled) return;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run());
        }

        /// <summary>Scale factor at normalized time t: 1 at both ends, peak at t = 0.5.</summary>
        public static float Evaluate(float t, float peak) => 1f + (peak - 1f) * Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);

        private IEnumerator Run()
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                transform.localScale = baseScale * Evaluate(t / duration, peakScale);
                yield return null;
            }
            transform.localScale = baseScale;
            routine = null;
        }
    }
}
