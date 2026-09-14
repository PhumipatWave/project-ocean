using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Generic text-only notification toast. Subscribes to NotificationManager.OnNotify
    /// and shows whatever message it receives — fade in, hold, fade out.
    /// Works for any event in the game (quest, inventory, save, etc.) as long as
    /// the caller does: NotificationManager.Notify("message here");
    ///
    /// If a new notification arrives while one is already showing/fading, the
    /// trigger is ignored (dropped) — the next one is only accepted once the
    /// current toast has fully disappeared. This prevents spam-clicking a
    /// single action from queuing up a long chain of toasts.
    ///
    /// Also handles NotificationManager's persistent channel (ShowPersistent/
    /// ClearPersistent — e.g. a tutorial prompt): a persistent message shows
    /// immediately and stays up (no auto-hide) until cleared, and takes priority
    /// over transient toasts — a Notify() that arrives while persistent is active
    /// is dropped rather than interrupting it.
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

        private void HandleNotify(string message)
        {
            // A persistent message (e.g. tutorial prompt) always wins — a transient
            // toast arriving during one is dropped rather than interrupting it.
            if (_persistentActive) return;

            // Ignore spam triggers while a notification is already showing/fading —
            // the next one is only accepted after the current one fully disappears.
            if (IsShowing) return;

            _activeRoutine = StartCoroutine(ToastRoutine(message));
        }

        private void HandlePersistentChanged(string message)
        {
            _persistentActive = !string.IsNullOrEmpty(message);

            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }

            if (_persistentActive)
            {
                if (messageText != null) messageText.text = message;
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
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
