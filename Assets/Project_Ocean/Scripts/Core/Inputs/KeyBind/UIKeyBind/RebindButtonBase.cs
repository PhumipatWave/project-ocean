using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    /// <summary>Base for a settings button that rebinds one key and shows its current binding.</summary>
    public abstract class RebindButtonBase : MonoBehaviour
    {
        [SerializeField] protected TMP_Text keyText;
        [SerializeField] protected Button rebindButton;
        [SerializeField] protected KeyBindErrorUI errorUI;

        /// <summary>Starts the interactive rebind; the manager calls back with (bindingPath, success).</summary>
        protected abstract void BeginRebind(KeyBindManager manager, Action<string, bool> onComplete);

        protected abstract string GetDisplayString(KeyBindManager manager);

        // Settings panels start inactive, so refresh each time the button is shown.
        private void OnEnable()
        {
            KeyBindManager.OnBindingsChanged += UpdateKeyText;
            if (rebindButton != null)
                rebindButton.onClick.AddListener(StartRebind);
            UpdateKeyText();
        }

        private void OnDisable()
        {
            KeyBindManager.OnBindingsChanged -= UpdateKeyText;
            if (rebindButton != null)
                rebindButton.onClick.RemoveListener(StartRebind);
        }

        private void StartRebind()
        {
            var manager = KeyBindManager.Instance;
            if (manager == null)
                return;

            // The button that just finished a rebind can reach this as a Submit; don't start another.
            if (KeyBindManager.IsRebinding || Time.frameCount - KeyBindManager.LastRebindFrame < 2)
                return;

            keyText.text = "Press Key...";
            if (rebindButton != null)
                rebindButton.interactable = false;

            BeginRebind(manager, OnRebindComplete);
        }

        private void OnRebindComplete(string bindingPath, bool success)
        {
            UpdateKeyText();

            // An empty path means the rebind was cancelled, not a clash.
            if (!success && !string.IsNullOrEmpty(bindingPath) && errorUI != null)
                errorUI.Show("Key already assigned!");

            if (rebindButton != null)
                rebindButton.interactable = true;
        }

        private void UpdateKeyText()
        {
            if (keyText == null)
                return;

            var manager = KeyBindManager.Instance;
            keyText.text = manager != null ? GetDisplayString(manager) : "---";
        }
    }
}
