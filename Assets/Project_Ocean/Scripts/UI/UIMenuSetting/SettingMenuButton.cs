using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class SettingMenuButton : MonoBehaviour
    {
        public void OpenSetting()
        {
            if (UIManager.Instance == null)
            {
                Debug.LogWarning("[SettingMenuButton] UIManager.Instance is null.");
                return;
            }

            UIManager.Instance.OpenSettingMenu();
        }
    }
}