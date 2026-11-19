using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Binds a volume slider to AudioManager for its channel, so every settings panel shows the same value.</summary>
    [RequireComponent(typeof(Slider))]
    public class AudioSliderBinder : MonoBehaviour
    {
        [SerializeField] private AudioChannel channel;

        private Slider slider;

        private void Awake() => slider = GetComponent<Slider>();

        private void OnEnable()
        {
            // Re-read on every open. Set with notify, before our listener is added, so the "NN%" label updates without an echo.
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
