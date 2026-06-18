using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// Binds Land-scene UI buttons to global singleton managers at runtime,
    /// avoiding cross-scene Inspector references that break on scene reload.
    public class LandUI : MonoBehaviour
    {
        [Header("Dive Panel")]
        [SerializeField] private Button startDiveButton;

        private void Start()
        {
            if (startDiveButton != null)
                startDiveButton.onClick.AddListener(() => GameManager.Instance?.StartDive());
        }
    }
}
