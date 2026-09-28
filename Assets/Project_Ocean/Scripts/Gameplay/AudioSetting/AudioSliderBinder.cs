using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Put on a volume Slider: shows the saved volume for its channel and writes changes back to
    /// AudioManager. Every settings panel (main menu, in-game pause) uses this, so they always agree
    /// and no panel needs a scene reference to an AudioManager.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class AudioSliderBinder : MonoBehaviour
    {
        [SerializeField] private AudioChannel channel;

        private Slider slider;

        private void Awake() => slider = GetComponent<Slider>();

        private void OnEnable()
        {
            // Panels are often inactive at startup and opened later — re-read the saved value each time.
            // Set it WITH notify (before our own listener is added, so there's no echo back to the manager)
            // so other listeners — e.g. the "NN%" label — update too; SetValueWithoutNotify left it stale.
            if (AudioManager.Instance != null)
                slider.value = AudioManager.Instance.GetVolume(channel);

            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable() => slider.onValueChanged.RemoveListener(OnValueChanged);

        private void OnValueChanged(float value)
        {
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.SetVolume(channel, value);
            AudioManager.Instance.PlayPreview(channel); // hear the new level (opening the panel never gets here)
        }
    }
}
