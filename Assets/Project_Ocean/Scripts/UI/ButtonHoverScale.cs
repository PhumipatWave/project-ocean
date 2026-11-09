using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Scale")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float speed = 10f;

        [Header("Button Text")]
        [SerializeField] private TMP_Text buttonText;

        [Header("Text")]
        [SerializeField] private string hoverText = "Upgrade";

        private Vector3 originalScale;
        private Vector3 targetScale;

        private void Awake()
        {
            originalScale = transform.localScale;

            ResetButton();
        }

        private void OnEnable()
        {
            // A panel closed mid-hover would otherwise reopen with the button still enlarged.
            ResetButton();

            RefreshPrice();
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.unscaledDeltaTime * speed
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = originalScale * hoverScale;

            if (buttonText != null)
            {
                buttonText.text = hoverText;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetButton();
            RefreshPrice();
        }

        /// <summary>Snaps the button back to its normal size.</summary>
        public void ResetButton()
        {
            targetScale = originalScale;
            transform.localScale = originalScale;
        }

        public void RefreshPrice()
        {
            if (buttonText == null)
                return;

            UpgradeSlot upgradeSlot =
                GetComponentInParent<UpgradeSlot>();

            if (upgradeSlot == null)
                return;

            buttonText.text =
                upgradeSlot.GetCurrentPriceText();
        }
    }
}
