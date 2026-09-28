using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Shows one notification at a time: fade in, hold, fade out.</summary>
    public class ToastNotificationView : NotificationView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image icon;
        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeDuration = 0.4f;

        private Coroutine routine;
        private string currentText;
        private NotificationMessage pending;
        private bool holding;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // The coroutine dies with the object; without this reset the view would stay "busy" forever.
            routine = null;
            currentText = null;
            pending = null;
            holding = false;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        protected override void Show(NotificationMessage msg)
        {
            if (routine != null)
            {
                // Same text again = spam, drop it. A different one waits (latest wins).
                if (msg.Text != currentText) pending = msg;
                return;
            }

            routine = StartCoroutine(ToastRoutine(msg));
        }

        private IEnumerator ToastRoutine(NotificationMessage msg)
        {
            currentText = msg.Text;
            if (messageText != null) messageText.text = msg.Text;
            if (icon != null)
            {
                icon.sprite = msg.Icon;
                icon.gameObject.SetActive(msg.Icon != null);
            }

            yield return Fade(0f, 1f);
            if (msg.Sticky)
            {
                holding = true;
                yield return new WaitWhile(() => holding);
            }
            else
                yield return new WaitForSecondsRealtime(showDuration);
            yield return Fade(1f, 0f);

            routine = null;
            currentText = null;

            if (pending != null)
            {
                var next = pending;
                pending = null;
                Show(next);
            }
        }

        protected override void Clear()
        {
            holding = false;
            // A sticky still waiting would otherwise show after its reason is gone and never end.
            if (pending != null && pending.Sticky) pending = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
