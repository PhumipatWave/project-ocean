using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.Testing
{
    public class LoadingUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text loadingText;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [Header("Fade")]
        [SerializeField] private float fadeSpeed = 4f;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            SetProgress(0f);
            SetLoadingText("Loading...");
        }

        public void SetProgress(float value)
        {
            if (progressBar == null)
                return;

            progressBar.value = Mathf.Clamp01(value);
        }

        public void SetLoadingText(string text)
        {
            if (loadingText == null)
                return;

            loadingText.text = text;
        }

        public void PlayAnimation()
        {
            if (animator == null)
                return;

            animator.Play("LoadingRotate");
        }

        public void ShowInstant()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void HideInstant()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
