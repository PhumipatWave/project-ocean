using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// Generic "Are you sure?" modal. Call Show() with a message and callbacks.
    /// Wire ConfirmButton.onClick → Confirm() and CancelButton.onClick → Cancel() in the Inspector.
    public class ConfirmationDialog : MonoBehaviour
    {
        public static ConfirmationDialog Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action _onConfirm;
        private Action _onCancel;
        private bool _isShowing;

        private void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        /// Show the dialog with a message. onCancel is optional.
        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            if (messageText != null) messageText.text = message;
            if (panel != null) panel.SetActive(true);

            if (!_isShowing)
            {
                _isShowing = true;
                PlayerUIState.Open();
            }
        }

        /// Wire to ConfirmButton.onClick in the Inspector.
        public void Confirm()
        {
            Hide();
            _onConfirm?.Invoke();
        }

        /// Wire to CancelButton.onClick in the Inspector.
        public void Cancel()
        {
            Hide();
            _onCancel?.Invoke();
        }

        private void Hide()
        {
            if (panel != null) panel.SetActive(false);

            if (_isShowing)
            {
                _isShowing = false;
                PlayerUIState.Close();
            }
        }
    }
}
