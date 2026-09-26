using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>The shared dialogue panel's widgets; gameplay drives it through IDialogueView.</summary>
    public class DialoguePanelView : MonoBehaviour, IDialogueView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image avatar;

        public GameObject Panel => panel;
        public bool IsOpen => panel != null && panel.activeSelf;

        public void SetOpen(bool open)
        {
            if (panel != null) panel.SetActive(open);
        }

        public void SetSpeaker(string characterId, string fallbackName = null, Sprite fallbackPortrait = null) =>
            DialogueSpeaker.Apply(nameText, avatar, characterId, fallbackName, fallbackPortrait);

        public void SetText(string text)
        {
            if (bodyText != null) bodyText.text = text;
        }
    }
}
