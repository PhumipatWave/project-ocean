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
    ///   until explicitly cleared, then fades out like a toast. Takes priority: while active, an
    ///   incoming toast is dropped rather than interrupting it.
    ///
    /// If a toast arrives while one is already showing/fading, a repeat of the same text is
    /// ignored (spam-clicking one action never queues a chain). A DIFFERENT message waits in
    /// a single pending slot (latest wins) and plays once the current toast is gone — so e.g.
    /// the auto-save "Game Saved!" on landing no longer swallows a message raised right after.
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
        private Coroutine _pulseRoutine;
        private bool _persistentActive;
        private string _currentMessage;
        private string _pendingMessage;

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
            NotificationManager.OnPersistentPulse += HandlePersistentPulse;
        }

        private void OnDisable()
        {
            NotificationManager.OnNotify -= HandleNotify;
            NotificationManager.OnPersistentChanged -= HandlePersistentChanged;
            NotificationManager.OnPersistentPulse -= HandlePersistentPulse;
        }

        private void HandlePersistentChanged(string message, Sprite sprite)
        {
            bool wasActive = _persistentActive;
            _persistentActive = !string.IsNullOrEmpty(message);

            if (!_persistentActive)
            {
                // Cleared: fade out like a normal toast, keeping the old text. Repeated
                // clears (or a toast already in flight) must not restart/cut it short.
                if (wasActive && isActiveAndEnabled)
                {
                    if (_activeRoutine != null) StopCoroutine(_activeRoutine); // mid fade-in
                    _activeRoutine = StartCoroutine(FadeToRoutine(0f));
                }
                else if (_activeRoutine == null && canvasGroup != null)
                    canvasGroup.alpha = 0f;
                return;
            }

            // A persistent message pre-empts any toast in flight — the guide prompt owns
            // the slot until cleared, not a timed fade.
            _pendingMessage = null;
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }

            if (messageText != null) messageText.text = message;
            if (isActiveAndEnabled) _activeRoutine = StartCoroutine(FadeToRoutine(1f));
            else if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        // Quick blink on the already-visible persistent banner — draws the eye back to it
        // when a blocked key press would otherwise fail silently (a Notify() toast is
        // dropped while a persistent message owns the slot, so it can't be used here).
        private void HandlePersistentPulse()
        {
            if (!_persistentActive || !isActiveAndEnabled || canvasGroup == null) return;
            if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
            _pulseRoutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            canvasGroup.alpha = 0.2f;
            yield return new WaitForSecondsRealtime(0.1f);
            canvasGroup.alpha = 1f;
            _pulseRoutine = null;
        }

        private IEnumerator FadeToRoutine(float target)
        {
            float start = canvasGroup != null ? canvasGroup.alpha : 0f;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = target;
            _activeRoutine = null;
        }

        private void HandleNotify(string message)
        {
            // Persistent (tutorial/guide) messages take priority — drop toasts while one
            // is active rather than interrupting it.
            if (_persistentActive) return;

            if (IsShowing)
            {
                if (message != _currentMessage) _pendingMessage = message;
                return;
            }

            _activeRoutine = StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            _currentMessage = message;
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
            _currentMessage = null;

            if (_pendingMessage != null)
            {
                var next = _pendingMessage;
                _pendingMessage = null;
                HandleNotify(next);
            }
        }
    }
}
