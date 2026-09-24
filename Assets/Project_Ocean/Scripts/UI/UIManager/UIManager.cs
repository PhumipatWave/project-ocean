using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Testing
{
    /// <summary>
    /// Central controller for blocking UI.
    ///
    /// Each UI registers itself through UIWindow.
    /// UIManager does not need Inspector references to individual panels.
    ///
    /// Supported UI:
    /// MainMenu
    /// PauseMenu
    /// SettingMenu   = Setting opened from Main Menu
    /// SettingPause  = Setting opened from Pause Menu
    /// Inventory
    /// Quest
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public enum UIType
        {
            None,
            MainMenu,
            PauseMenu,
            SettingMenu,
            SettingPause,
            Inventory,
            Quest
        }

        private readonly Dictionary<UIType, UIWindow> windows =
            new Dictionary<UIType, UIWindow>();

        private UIType currentUI = UIType.None;

        private bool uiManagerOwnsLock;
        private bool uiManagerOpenedLock; // false when someone else (e.g. a cutscene) already held it
        private int lastOpenFrame = -1;

        [Header("Behaviour")]
        [SerializeField] private bool pauseGameWhenUIOpen = true;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        [Header("Background Dim")]
        [SerializeField] private UIFadeBackground backgroundDim;

        public UIType CurrentUI => currentUI;

        public bool IsUIOpen => currentUI != UIType.None;

        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Do not reset PlayerUIState here if another persistent
            // UI system may already own it.
            if (!PlayerUIState.IsLocked)
            {
                PlayerUIState.Reset();
            }
        }

        private void Start()
        {
            RefreshRegisteredWindows();
        }

        private void Update()
        {
            HandleEscapeInput();
        }

        // =========================================================
        // REGISTER
        // =========================================================

        public void Register(UIWindow window)
        {
            if (window == null)
                return;

            UIType type = ConvertType(window.Type);

            if (type == UIType.None)
                return;

            if (windows.TryGetValue(type, out UIWindow existing))
            {
                if (existing != null && existing != window)
                {
                    LogWarning(
                        $"UIType '{type}' is already registered by " +
                        $"'{existing.name}'. Replacing it with '{window.name}'."
                    );
                }
            }

            windows[type] = window;

            Log($"Registered: {type} -> {window.name}");
        }

        public void Unregister(UIWindow window)
        {
            if (window == null)
                return;

            UIType type = ConvertType(window.Type);

            if (windows.TryGetValue(type, out UIWindow registered))
            {
                // Only remove the entry if THIS window is the one registered.
                if (registered == window)
                {
                    windows.Remove(type);

                    // Only clear the current UI if this exact registered
                    // window was the current UI.
                    if (currentUI == type)
                    {
                        currentUI = UIType.None;
                        RemoveBlockingState();
                    }
                }
            }

            Log($"Unregistered: {type} -> {window.name}");
        }

        private void RefreshRegisteredWindows()
        {
            UIWindow[] found =
                FindObjectsByType<UIWindow>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (UIWindow window in found)
            {
                Register(window);
            }

            Log($"Registered UI count: {windows.Count}");
        }

        private bool TryGetWindow(
            UIType type,
            out UIWindow window)
        {
            if (windows.TryGetValue(type, out window))
            {
                if (window != null)
                    return true;

                windows.Remove(type);
            }

            // Useful when an additive Scene has just loaded.
            RefreshRegisteredWindows();

            return windows.TryGetValue(type, out window)
                   && window != null;
        }

        // =========================================================
        // OPEN
        // =========================================================

        public void Open(UIType type)
        {
            if (type == UIType.None)
                return;

            if (!TryGetWindow(type, out UIWindow target))
            {
                Debug.LogWarning(
                    $"[UIManager] Cannot open '{type}'. " +
                    "No matching UIWindow was found in the loaded Scenes."
                );

                return;
            }

            // Already open.
            if (currentUI == type && target.gameObject.activeSelf)
            {
                target.transform.SetAsLastSibling();
                return;
            }

            // Close the currently managed panel only.
            // Keep the blocking state while switching directly
            // from one UI to another.
            CloseCurrentPanelOnly();

            target.gameObject.SetActive(true);
            target.transform.SetAsLastSibling();

            currentUI = type;
            lastOpenFrame = Time.frameCount;

            ApplyBlockingState();

            Log($"Open: {type} -> {target.name}");
        }

        // =========================================================
        // CLOSE
        // =========================================================

        public void CloseCurrent()
        {
            if (currentUI == UIType.None)
                return;

            CloseCurrentPanelOnly();

            currentUI = UIType.None;

            RemoveBlockingState();

            Log("Close Current");
        }

        private void CloseCurrentPanelOnly()
        {
            if (currentUI == UIType.None)
                return;

            if (windows.TryGetValue(currentUI, out UIWindow window))
            {
                if (window != null)
                {
                    window.gameObject.SetActive(false);
                }
            }
        }

        public void Close(UIType type)
        {
            if (windows.TryGetValue(type, out UIWindow window))
            {
                if (window != null)
                {
                    window.gameObject.SetActive(false);
                }
            }

            if (currentUI == type)
            {
                currentUI = UIType.None;
                RemoveBlockingState();
            }

            Log($"Close: {type}");
        }

        public void Toggle(UIType type)
        {
            if (currentUI == type)
            {
                CloseCurrent();
            }
            else
            {
                Open(type);
            }
        }

        public void CloseAll()
        {
            foreach (KeyValuePair<UIType, UIWindow> pair in windows)
            {
                if (pair.Value != null)
                {
                    pair.Value.gameObject.SetActive(false);
                }
            }

            currentUI = UIType.None;

            RemoveBlockingState();

            Log("Close All");
        }

        // =========================================================
        // MAIN MENU
        // =========================================================

        public void OpenMainMenu()
        {
            Open(UIType.MainMenu);
        }

        public void ToggleMainMenu()
        {
            Toggle(UIType.MainMenu);
        }

        // =========================================================
        // PAUSE MENU
        // =========================================================

        public void OpenPauseMenu()
        {
            // ถ้ามี UI อื่นเปิดอยู่ ห้ามเปิด Pause Menu ซ้อน
            if (currentUI != UIType.None)
                return;

            // ถ้ามี UI จากระบบอื่นกำลังเปิดอยู่
            // เช่น Quest Board / Shop / Upgrade / Interaction
            // (a playing cutscene holds the lock too, but Pause is allowed over it)
            if (PlayerUIState.IsLocked && !CutsceneGate.IsPlaying)
                return;

            Open(UIType.PauseMenu);
        }

        public void TogglePauseMenu()
        {
            // ถ้า Pause Menu เปิดอยู่ ให้กด Menu เพื่อปิดได้
            if (currentUI == UIType.PauseMenu)
            {
                CloseCurrent();
                return;
            }

            // มี UI อื่นของ UIManager เปิดอยู่ → ห้ามเปิด Pause ซ้อน
            if (currentUI != UIType.None)
                return;

            // มี UI จากระบบอื่นเปิดอยู่ → ห้ามเปิด Pause (a playing cutscene is the exception)
            if (PlayerUIState.IsLocked && !CutsceneGate.IsPlaying)
                return;

            Open(UIType.PauseMenu);
        }

        // =========================================================
        // SETTING
        // =========================================================

        /// <summary>
        /// Opens SettingPanel_Menu from the Main Menu.
        /// </summary>
        public void OpenSettingMenu()
        {
            Open(UIType.SettingMenu);
        }

        public void ToggleSettingMenu()
        {
            Toggle(UIType.SettingMenu);
        }

        /// <summary>
        /// Opens SettingPanel from the in-game Pause Menu.
        /// </summary>
        public void OpenSettingPause()
        {
            Open(UIType.SettingPause);
        }

        public void ToggleSettingPause()
        {
            Toggle(UIType.SettingPause);
        }

        /// <summary>
        /// Kept for compatibility with old Button OnClick events.
        /// If an old button still calls OpenSetting(), it will open
        /// the Pause/in-game Setting.
        /// </summary>
        public void OpenSetting()
        {
            OpenSettingPause();
        }

        // =========================================================
        // INVENTORY
        // =========================================================

        public void OpenInventory()
        {
            Open(UIType.Inventory);
        }

        public void ToggleInventory()
        {
            Toggle(UIType.Inventory);
        }

        // =========================================================
        // QUEST
        // =========================================================

        public void OpenQuest()
        {
            Open(UIType.Quest);
        }

        public void ToggleQuest()
        {
            Toggle(UIType.Quest);
        }

        // =========================================================
        // ESC
        // =========================================================

        private void HandleEscapeInput()
        {
            if (Keyboard.current == null)
                return;

            if (!Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            // Close UIManager-owned UI first.
            // (Not on the frame it opened: the Menu input action also opens the pause menu on Esc.)
            if (IsUIOpen)
            {
                if (lastOpenFrame != Time.frameCount)
                    CloseCurrent();
                return;
            }

            // Other blocking systems such as Quest Board,
            // Shop, Upgrade, Interaction, etc.
            if (PlayerUIState.IsLocked)
            {
                PlayerUIState.CloseCurrent();
            }
        }

        // =========================================================
        // PLAYER LOCK / TIME SCALE
        // =========================================================

        private void ApplyBlockingState()
        {
            // Take ownership of the lock once, when UIManager first opens a panel.
            if (!uiManagerOwnsLock)
            {
                if (!PlayerUIState.IsLocked)
                {
                    PlayerUIState.Open();
                    uiManagerOpenedLock = true;
                }

                uiManagerOwnsLock = true;
            }

            if (pauseGameWhenUIOpen)
            {
                Time.timeScale = 0f;
            }

            ShowBackgroundDim();
        }

        private void RemoveBlockingState()
        {
            // Only undo the pause if UIManager actually applied it — an unconditional reset
            // would stomp aim slow-mo or another system's pause (e.g. a chest panel).
            if (uiManagerOwnsLock)
            {
                if (uiManagerOpenedLock)
                    PlayerUIState.Close();
                uiManagerOwnsLock = false;
                uiManagerOpenedLock = false;

                if (pauseGameWhenUIOpen)
                    Time.timeScale = 1f; // not a saved snapshot: it could be mid aim slow-mo
            }

            HideBackgroundDim();
        }

        // =========================================================
        // ENUM CONVERSION
        // =========================================================

        private UIType ConvertType(UIWindow.UIType type)
        {
            switch (type)
            {
                case UIWindow.UIType.MainMenu:
                    return UIType.MainMenu;

                case UIWindow.UIType.PauseMenu:
                    return UIType.PauseMenu;

                case UIWindow.UIType.SettingMenu:
                    return UIType.SettingMenu;

                case UIWindow.UIType.SettingPause:
                    return UIType.SettingPause;

                case UIWindow.UIType.Inventory:
                    return UIType.Inventory;

                case UIWindow.UIType.Quest:
                    return UIType.Quest;

                default:
                    return UIType.None;
            }
        }

        // =========================================================
        // BACKGROUND DIM
        // =========================================================

        private void ShowBackgroundDim()
        {
            if (backgroundDim != null)
            {
                backgroundDim.Show();
            }
        }

        private void HideBackgroundDim()
        {
            if (backgroundDim != null)
            {
                backgroundDim.Hide();
            }
        }

        // =========================================================
        // DEBUG
        // =========================================================

        private void Log(string message)
        {
            if (debugLogs)
            {
                Debug.Log($"[UIManager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (debugLogs)
            {
                Debug.LogWarning($"[UIManager] {message}");
            }
        }
    }
}
