using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Widgets of an interact speech popup; gameplay drives it through IInteractPopupView.</summary>
    public class InteractPopupView : MonoBehaviour, IInteractPopupView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image portrait;
        [SerializeField] private Image extraImage;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Show(string speaker, string text, Sprite portraitSprite, Sprite extraSprite = null)
        {
            if (panel == null) return;

            if (nameText != null && speaker != null) nameText.text = speaker;
            if (bodyText != null) bodyText.text = text;
            SetImage(portrait, portraitSprite);
            SetImage(extraImage, extraSprite);
            panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            if (extraImage != null) extraImage.gameObject.SetActive(false);
        }

        private static void SetImage(Image image, Sprite sprite)
        {
            if (image == null) return;

            image.gameObject.SetActive(sprite != null);
            if (sprite != null) image.sprite = sprite;
        }
    }
}
