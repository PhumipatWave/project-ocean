using System.Collections;
using System.Collections.Generic;
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
    /// If a new notification arrives while one is showing, it is queued and
    /// shown after the current one finishes (so messages never get lost/cut off).
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

        private readonly Queue<string> _pending = new Queue<string>();
        private Coroutine _activeRoutine;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void OnEnable()  => NotificationManager.OnNotify += HandleNotify;
        private void OnDisable() => NotificationManager.OnNotify -= HandleNotify;

        private void HandleNotify(string message)
        {
            _pending.Enqueue(message);

            if (_activeRoutine == null)
                _activeRoutine = StartCoroutine(ToastRoutine());
        }

        private IEnumerator ToastRoutine()
        {
            while (_pending.Count > 0)
            {
                string message = _pending.Dequeue();
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
            }

            _activeRoutine = null;
        }
    }
}
