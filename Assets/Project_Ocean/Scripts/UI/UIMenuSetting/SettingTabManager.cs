using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class SettingTabManager : MonoBehaviour
    {
        [Header("Pages")]
        [SerializeField] private GameObject generalSetting;
        [SerializeField] private GameObject keyBindPanel;

        [Header("Buttons")]
        [SerializeField] private Button settingButton;
        [SerializeField] private Button controlsButton;

        [Header("Button Colors")]
        [SerializeField] private Color activeColor = new Color(1f, 0.65f, 0.1f);
        [SerializeField] private Color inactiveColor = new Color(0.25f, 0.25f, 0.25f);

        private void Start()
        {
            OpenSetting();
        }

        public void OpenSetting()
        {
            generalSetting.SetActive(true);
            keyBindPanel.SetActive(false);

            SetButtonColors(settingButton, controlsButton);
        }

        public void OpenControls()
        {
            generalSetting.SetActive(false);
            keyBindPanel.SetActive(true);

            SetButtonColors(controlsButton, settingButton);
        }

        private void SetButtonColors(Button activeButton, Button inactiveButton)
        {
            activeButton.image.color = activeColor;
            inactiveButton.image.color = inactiveColor;
        }
    }
}
