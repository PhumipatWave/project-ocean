using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    public class KeyBindResetAllButton : MonoBehaviour
    {
        [SerializeField] private Button resetAllButton;

        private void Start()
        {
            resetAllButton.onClick.AddListener(ResetAll);
        }

        private void OnDestroy()
        {
            if (resetAllButton != null)
                resetAllButton.onClick.RemoveListener(ResetAll);
        }

        private void ResetAll()
        {
            if (KeyBindManager.Instance == null)
                return;

            KeyBindManager.Instance.ResetAllToDefault();
        }
    }
}