using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>
    /// Attach to a Land UI panel that an InteractUI's onInteract reveals (SetActive(true)) and whose
    /// own Close button already does SetActive(false) + LandUI.CloseUI(). The InteractUI already owns
    /// PlayerUIState's lock for this panel — this only registers how to close it, so the Esc key can
    /// trigger the same close path.
    /// </summary>
    public class EscClosablePanel : MonoBehaviour
    {
        private void OnEnable()
        {
            PlayerUIState.SetCloser(() =>
            {
                gameObject.SetActive(false);
                PlayerUIState.Close();
            });
        }
    }
}
