using UnityEngine;

namespace Game.UI
{
    /// <summary>Greys out the Continue button when no save exists.</summary>
    public class ContinueButtonGate : MonoBehaviour
    {
        // SaveMenuUI sits on the save panel, which starts inactive, so it can't check on menu load itself.
        [SerializeField] private SaveMenuUI saveMenu;

        private void OnEnable()
        {
            if (saveMenu != null)
                saveMenu.RefreshContinueButton();
        }
    }
}
