using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Core
{
    public class KeyBindUI : MonoBehaviour
    {
        [SerializeField] private KeyAction action;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private Button rebindButton;
        

        private void Start()
        {
            UpdateKeyText();

            rebindButton.onClick.AddListener(StartRebind);
           
            KeyBindManager.OnBindingsChanged += UpdateKeyText;
        }

        private void OnDestroy()
        {
            if (rebindButton != null)
                rebindButton.onClick.RemoveListener(StartRebind);     
            KeyBindManager.OnBindingsChanged -= UpdateKeyText;
        }

        private void StartRebind()
        {
            keyText.text = "Press Key...";

            rebindButton.interactable = false;

            KeyBindManager.Instance.StartRebind(
                action,
                OnRebindComplete
            );
        }

        private void OnRebindComplete(
            string bindingPath,
            bool success)
        {
            if (success)
            {
                UpdateKeyText();
            }
            else
            {
                UpdateKeyText();

                Debug.LogWarning(
                    "This key is already assigned."
                );
            }

            rebindButton.interactable = true;
        }

        private void UpdateKeyText()
        {
            if (KeyBindManager.Instance == null)
            {
                keyText.text = "---";
                return;
            }

            var inputAction =
                KeyBindManager.Instance.GetAction(action);

            if (inputAction == null)
            {
                keyText.text = "---";
                return;
            }

            keyText.text =
               inputAction.GetBindingDisplayString(
                   0,
                   InputBinding.DisplayStringOptions.DontIncludeInteractions
               );
        }
    }
}