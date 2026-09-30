using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class UIFadeBackground : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField, Range(0f, 1f)]
        private float dimAlpha = 0.55f;

        [SerializeField]
        private float fadeSpeed = 8f;

        private CanvasGroup canvasGroup;

        private float targetAlpha;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;

            targetAlpha = 0f;

            // The dim overlay must not block clicks on the buttons above it.
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void Update()
        {
            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha,
                targetAlpha,
                Time.unscaledDeltaTime * fadeSpeed
            );
        }

        public void Show()
        {
            targetAlpha = dimAlpha;
        }

        public void Hide()
        {
            targetAlpha = 0f;
        }

        public void SetDim(float alpha)
        {
            dimAlpha = Mathf.Clamp01(alpha);
        }
    }
}