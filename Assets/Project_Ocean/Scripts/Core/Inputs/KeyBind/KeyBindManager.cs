using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
    public class KeyBindManager : MonoBehaviour
    {
        public static KeyBindManager Instance { get; private set; }

        public static event Action OnInitialized;

        public static event Action OnBindingsChanged;

        private InputSystem_Actions inputActions;

        public InputSystem_Actions Actions => inputActions;

        private string SavePath =>
           Path.Combine(
        Application.persistentDataPath,
        "keybinds.json"
           );

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            inputActions = new InputSystem_Actions();

            LoadBindings();

            OnInitialized?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            inputActions?.Dispose();
            Instance = null;
        }

        public bool IsBindingUsed(
            string bindingPath,
            KeyAction excludeAction)
        {
            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                if (action == excludeAction)
                    continue;

                InputAction inputAction = GetAction(action);

                if (inputAction == null)
                    continue;

                foreach (var binding in inputAction.bindings)
                {
                    if (binding.effectivePath == bindingPath)
                        return true;
                }
            }

            return false;
        }

        public InputAction GetAction(KeyAction action)
        {
            return action switch
            {
                KeyAction.Move =>
                    inputActions.Player.Move,

                KeyAction.Interact =>
                    inputActions.Player.Interact,

                KeyAction.Inventory =>
                    inputActions.Player.Inventory,

                KeyAction.SwitchItem =>
                    inputActions.Player.SwitchItem,

                KeyAction.UseItem =>
                    inputActions.Player.ItemConsume,

                KeyAction.Sprint =>
                    inputActions.Player.Sprint,

                KeyAction.Attack =>
                    inputActions.Player.Attack,

                KeyAction.Aim =>
                    inputActions.Player.Aim,

                _ => null
            };
        }

        public void StartRebind(
            KeyAction action,
            Action<string, bool> onComplete)
        {
            InputAction inputAction = GetAction(action);

            if (inputAction == null)
            {
                Debug.LogWarning(
                    $"[KeyBind] Action not found: {action}"
                );

                return;
            }

            inputAction.Disable();

            inputAction.PerformInteractiveRebinding()
                .WithControlsExcluding("<Mouse>/position")
                .OnComplete(operation =>
                {
                    string bindingPath =
                        inputAction.bindings[0].effectivePath;

                    operation.Dispose();

                    if (IsBindingUsed(bindingPath, action))
                    {
                        inputAction.Enable();

                        Debug.LogWarning(
                            $"[KeyBind] {bindingPath} is already used."
                        );

                        onComplete?.Invoke(bindingPath, false);

                        return;
                    }

                    inputAction.Enable();

                    onComplete?.Invoke(bindingPath, true);

                    SaveBindings();

                    OnBindingsChanged?.Invoke();

                    Debug.Log(
                        $"[KeyBind] {action} → {bindingPath}"
                    );
                })
                .Start();
        }

        // Save Key Bind -----------------------------------------------
        public void SaveBindings()
        {
            KeyBindSettings settings = new KeyBindSettings();

            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                InputAction inputAction = GetAction(action);

                if (inputAction == null)
                    continue;

                KeyBindingData data = new KeyBindingData
                {
                    action = action.ToString()
                };

                foreach (var binding in inputAction.bindings)
                {
                    if (string.IsNullOrEmpty(binding.effectivePath))
                        continue;

                    data.bindingPaths.Add(binding.effectivePath);
                }

                if (data.bindingPaths.Count > 0)
                {
                    settings.bindings.Add(data);
                }
            }

            string json = JsonUtility.ToJson(
                settings,
                true
            );

            File.WriteAllText(
                SavePath,
                json
            );

            Debug.Log(
                $"[KeyBind] Saved: {SavePath}"
            );
        }

        public void LoadBindings()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log(
                    "[KeyBind] No saved bindings found. Using defaults."
                );

                return;
            }

            string json = File.ReadAllText(SavePath);

            KeyBindSettings settings =
                JsonUtility.FromJson<KeyBindSettings>(json);

            if (settings == null)
            {
                Debug.LogWarning(
                    "[KeyBind] Failed to read keybind settings."
                );

                return;
            }

            foreach (KeyBindingData binding in settings.bindings)
            {
                if (!Enum.TryParse(
                        binding.action,
                        out KeyAction action))
                {
                    continue;
                }

                InputAction inputAction =
                    GetAction(action);

                if (inputAction == null)
                    continue;

                for (int i = 0; i < binding.bindingPaths.Count; i++)
                {
                    if (i >= inputAction.bindings.Count)
                        break;

                    inputAction.ApplyBindingOverride(
                        i,
                        binding.bindingPaths[i]
                    );
                }
            }

            Debug.Log("[KeyBind] Bindings loaded.");
        }

        public void ResetAllToDefault()
        {
            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                InputAction inputAction = GetAction(action);

                if (inputAction == null)
                    continue;

                for (int i = 0; i < inputAction.bindings.Count; i++)
                {
                    inputAction.RemoveBindingOverride(i);
                }
            }

            SaveBindings();

            OnBindingsChanged?.Invoke();

            Debug.Log(
                "[KeyBind] All bindings reset to default."
            );
        }
    }
}