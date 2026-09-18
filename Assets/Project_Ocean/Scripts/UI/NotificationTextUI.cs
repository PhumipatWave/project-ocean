using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Text notification display. Handles both of NotificationManager's channels on one
    /// shared UI slot:
    /// - OnNotify: a timed toast (fade in, hold, fade out).
    /// - OnPersistentChanged: a tutorial/guide message that shows immediately and stays
    ///   until explicitly cleared — no fade, no timer. Takes priority: while active, an
    ///   incoming toast is dropped rather than interrupting it.
    ///
    /// If a toast arrives while one is already showing/fading, the trigger is ignored
    /// (dropped) — the next one is only accepted once the current toast has fully
    /// disappeared. This prevents spam-clicking a single action from queuing up a long
    /// chain of toasts.
    ///
    /// Put this on a UI GameObject with a CanvasGroup + TextMeshProUGUI, ideally
    /// on a persistent HUD canvas.
    /// </summary>
    public class NotificationTextUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI messageText;

        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeDuration  = 0.4f;

        private Coroutine _activeRoutine;
        private bool _persistentActive;

        /// <summary>True while a toast is visible or mid-fade. New toast triggers are ignored until this clears.</summary>
        private bool IsShowing => _activeRoutine != null;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            NotificationManager.OnNotify += HandleNotify;
            NotificationManager.OnPersistentChanged += HandlePersistentChanged;
        }

        private void OnDisable()
        {
            NotificationManager.OnNotify -= HandleNotify;
            NotificationManager.OnPersistentChanged -= HandlePersistentChanged;
        }

        private void HandlePersistentChanged(string message, Sprite sprite)
        {
            _persistentActive = !string.IsNullOrEmpty(message);

            // A persistent message pre-empts any toast in flight — the guide prompt owns
            // the slot until cleared, not a timed fade.
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }

            if (messageText != null) messageText.text = message;
            if (canvasGroup != null) canvasGroup.alpha = _persistentActive ? 1f : 0f;
        }

        private void HandleNotify(string message)
        {
            // Persistent (tutorial/guide) messages take priority — drop toasts while one
            // is active rather than interrupting it. Also ignore spam triggers while a
            // toast is already showing/fading — the next one is only accepted after the
            // current one fully disappears.
            if (_persistentActive || IsShowing) return;

            _activeRoutine = StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            if (messageText != null) messageText.text = message;

            // Fade in
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(showDuration);

            // Fade out
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            _activeRoutine = null;
        }
    }
}
