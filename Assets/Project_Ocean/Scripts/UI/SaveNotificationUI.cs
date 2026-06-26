using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    /// Small toast that appears briefly after a successful save then fades out.
    /// Lives on GameHUD — persists across all scenes.
    public class SaveNotificationUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI messageText;

        [SerializeField] private float showDuration  = 2f;
        [SerializeField] private float fadeDuration  = 0.4f;

        private Coroutine _activeRoutine;

        private void OnEnable()  => SaveBridge.OnSaved += Show;
        private void OnDisable() => SaveBridge.OnSaved -= Show;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void Show()
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(ToastRoutine());
        }

        private IEnumerator ToastRoutine()
        {
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
