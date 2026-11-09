using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>One line in a notification stack; fades out and destroys itself.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationLine : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeDuration = 0.4f;

        private CanvasGroup group;

        public void Setup(NotificationMessage msg)
        {
            group = GetComponent<CanvasGroup>();
            text.text = msg.Text;
            icon.sprite = msg.Icon;
            icon.gameObject.SetActive(msg.Icon != null);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return Fade(0f, 1f);
            yield return new WaitForSecondsRealtime(showDuration);
            yield return Fade(1f, 0f);
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
