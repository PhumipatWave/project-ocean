using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Opens and closes the blocking UI windows, which register themselves through UIWindow.</summary>
    public class UIManager : Singleton<UIManager>
    {

        public enum UIType
        {
            None,
            MainMenu,
            PauseMenu,
            SettingMenu,
            SettingPause,
            Inventory,
            Quest,
            FishCollection
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

        [Header("HUD Buttons")]
        [SerializeField] private GameObject menuButtonNew;
        [SerializeField] private GameObject fishCollectionButton;
        [SerializeField] private GameObject landButton;

        [Header("Player Area State")]
        [Tooltip("True while the player is underwater. Keep false on land.")]
        [SerializeField] private bool isUnderwater;

        public UIType CurrentUI => currentUI;

        public bool IsUIOpen => currentUI != UIType.None;

        protected override bool Persistent => true;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            // Do not reset PlayerUIState here if another persistent
            // UI system may already own it.
            if (!PlayerUIState.IsLocked)
            {
                PlayerUIState.Reset();
            }

            GameManager.OnReturningToMenu += CloseAll;
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
                GameManager.OnReturningToMenu -= CloseAll;
            base.OnDestroy();
        }

        private void Start()
        {
            RefreshRegisteredWindows();
        }

        private void Update()
        {
            HandleEscapeInput();
            RefreshHUDButtonsVisibility();
        }

        /// <summary>
        /// Hide the in-game HUD buttons whenever a UIManager window or another
        /// blocking UI system owns the UI lock. This also handles external panels
        /// such as shops or quest boards that use PlayerUIState.
        /// </summary>
        private void RefreshHUDButtonsVisibility()
        {
            // HUD controls are hidden whenever a blocking UI is open.
            bool noBlockingUI =
                currentUI == UIType.None &&
                !PlayerUIState.IsLocked;

            SetActiveIfNeeded(menuButtonNew, noBlockingUI);
            SetActiveIfNeeded(fishCollectionButton, noBlockingUI);

            // LandButton is only available underwater, and only when no UI is open.
            bool shouldShowLandButton = isUnderwater && noBlockingUI;
            SetActiveIfNeeded(landButton, shouldShowLandButton);
        }

        private static void SetActiveIfNeeded(GameObject target, bool shouldBeActive)
        {
            if (target != null && target.activeSelf != shouldBeActive)
            {
                target.SetActive(shouldBeActive);
            }
        }

        /// <summary>
        /// Call with true when entering the ocean and false when returning to land.
        /// The button is automatically hidden while any blocking UI is open.
        /// </summary>
        public void SetUnderwater(bool underwater)
        {
            isUnderwater = underwater;
            RefreshHUDButtonsVisibility();

            Log($"Underwater state: {isUnderwater}");
        }

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

            // Keep the blocking state while switching straight from one UI to another.
            CloseCurrentPanelOnly();

            target.gameObject.SetActive(true);
            target.transform.SetAsLastSibling();

            currentUI = type;
            lastOpenFrame = Time.frameCount;

            ApplyBlockingState();

            Log($"Open: {type} -> {target.name}");
        }

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

        public void OpenMainMenu()
        {
            Open(UIType.MainMenu);
        }

        public void ToggleMainMenu()
        {
            Toggle(UIType.MainMenu);
        }

        public void OpenPauseMenu()
        {
            if (currentUI != UIType.None)
                return;

            // Another system's UI (quest board, shop, upgrade) holds the lock; a cutscene may be paused.
            if (PlayerUIState.IsLocked && !CutsceneGate.IsPlaying)
                return;

            Open(UIType.PauseMenu);
        }

        public void TogglePauseMenu()
        {
            if (currentUI == UIType.PauseMenu)
            {
                CloseCurrent();
                return;
            }

            if (currentUI != UIType.None)
                return;

            // Another system's UI holds the lock; a cutscene may be paused.
            if (PlayerUIState.IsLocked && !CutsceneGate.IsPlaying)
                return;

            Open(UIType.PauseMenu);
        }

        /// <summary>Opens Settings from the Main Menu.</summary>
        public void OpenSettingMenu()
        {
            Open(UIType.SettingMenu);
        }

        /// <summary>Opens Settings from the Pause Menu.</summary>
        public void OpenSettingPause()
        {
            Open(UIType.SettingPause);
        }

        /// <summary>Old buttons still call this; opens the in-game Settings.</summary>
        public void OpenSetting()
        {
            OpenSettingPause();
        }

        public void OpenInventory()
        {
            Open(UIType.Inventory);
        }

        public void OpenQuest()
        {
            Open(UIType.Quest);
        }

        public void OpenFishCollection()
        {
            Open(UIType.FishCollection);
        }

        public void ToggleFishCollection()
        {
            Toggle(UIType.FishCollection);
        }

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

                case UIWindow.UIType.FishCollection:
                    return UIType.FishCollection;

                default:
                    return UIType.None;
            }
        }

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

        private void Log(string message)
        {
            if (debugLogs)
            {
                GameLog.Info($"[UIManager] {message}");
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
