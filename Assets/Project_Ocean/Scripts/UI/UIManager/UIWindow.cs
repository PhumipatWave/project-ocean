using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class UIWindow : MonoBehaviour
    {
        public enum UIType
        {
            MainMenu,
            PauseMenu,
            SettingMenu,
            SettingPause,
            Inventory,
            Quest
        }

        [SerializeField]
        private UIType uiType;

        public UIType Type => uiType;

        private void Awake()
        {
            Register();
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDestroy()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Unregister(this);
            }
        }

        private void Register()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Register(this);
            }
        }
    }
}