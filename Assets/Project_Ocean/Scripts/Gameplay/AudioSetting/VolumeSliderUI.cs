using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;

namespace Game.Gameplay
{
    public class VolumeSliderUI : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text percentText;

        private void Start()
        {
            slider.onValueChanged.AddListener(UpdatePercent);
            UpdatePercent(slider.value);
        }

        private void UpdatePercent(float value)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            percentText.text = percent + "%";
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(UpdatePercent);
        }
    }
}
