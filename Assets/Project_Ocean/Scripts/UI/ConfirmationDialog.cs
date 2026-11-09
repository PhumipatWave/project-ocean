using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// Generic "Are you sure?" modal. Call Show() with a message and callbacks.
    /// Wire ConfirmButton.onClick → Confirm() and CancelButton.onClick → Cancel() in the Inspector.
    public class ConfirmationDialog : Singleton<ConfirmationDialog>, IModalPrompt
    {

        [SerializeField] private GameObject BGPanel;
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action _onConfirm;
        private Action _onCancel;
        private bool _isShowing;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;
            ModalPrompt.Register(this);
            if (panel != null) BGPanel.SetActive(false);
            if (panel != null) panel.SetActive(false);
        }

        // A menu (pause, settings) owns the screen — don't stack the prompt over it.
        public bool CanShow => UIManager.Instance == null || !UIManager.Instance.IsUIOpen;

        public bool IsShowing => _isShowing;

        private int _shownFrame = -1;
        public bool ShownThisFrame => _shownFrame == Time.frameCount;

        protected override void OnDestroy()
        {
            if (Instance == this)
                ModalPrompt.Unregister(this);
            base.OnDestroy();
        }

        /// Show the dialog with a message. onCancel is optional.
        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;
            _shownFrame = Time.frameCount;

            if (messageText != null) messageText.text = message;
            if (panel != null) BGPanel.SetActive(true);
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
            if (BGPanel != null) BGPanel.SetActive(false);
            if (panel != null) panel.SetActive(false);

            if (_isShowing)
            {
                _isShowing = false;
                PlayerUIState.Close();
            }
        }
    }
}
