using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// Binds all Land-scene buttons to global singleton managers at runtime,
    /// avoiding cross-scene Inspector references that break on scene reload.
    /// Also exposes Open/Close for wiring any land UI close button to PlayerUIState.
    public class LandUI : MonoBehaviour
    {
        /// <summary>Wire to any button that opens a blocking land UI.</summary>
        public void OpenUI()  => PlayerUIState.Open();

        /// <summary>Wire to every close/back button that fully dismisses a land UI.</summary>
        public void CloseUI() => PlayerUIState.Close();

        [Header("Dive Panel")]
        [SerializeField] private Button startDiveButton;

        [Header("Day Panel")]
        [SerializeField] private Button nextDayButton;

        [Header("Home Panel")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        private void Start()
        {
            if (startDiveButton != null)
                startDiveButton.onClick.AddListener(() => GameManager.Instance?.StartDive());

            if (nextDayButton != null)
                nextDayButton.onClick.AddListener(() => DayManager.Instance?.AdvanceDay());

            if (saveButton != null)
                saveButton.onClick.AddListener(() => SaveBridge.Instance?.SaveGame());

            if (loadButton != null)
                loadButton.onClick.AddListener(() => SaveBridge.Instance?.LoadGame());
        }
    }
}
