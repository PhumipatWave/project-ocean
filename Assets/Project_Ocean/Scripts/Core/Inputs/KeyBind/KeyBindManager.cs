using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
    public class KeyBindManager : Singleton<KeyBindManager>
    {

        public static event Action OnInitialized;

        public static event Action OnBindingsChanged;

        private InputSystem_Actions inputActions;

        public InputSystem_Actions Actions => inputActions;

        private string SavePath => Path.Combine(Application.persistentDataPath,"keybinds.json");

        protected override bool Persistent => true;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;

            inputActions = new InputSystem_Actions();

            LoadBindings();

            OnInitialized?.Invoke();
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
                inputActions?.Dispose();
            base.OnDestroy();
        }

        public bool IsBindingUsed(string bindingPath,KeyAction excludeAction,int excludeBindingIndex = -1)
        {
            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                InputAction inputAction = GetAction(action);

                if (inputAction == null)
                    continue;

                for (int i = 0; i < inputAction.bindings.Count; i++)
                {
                    
                    if (action == excludeAction &&
                        i == excludeBindingIndex)
                    {
                        continue;
                    }

                    var binding = inputAction.bindings[i];

                    if (binding.effectivePath == bindingPath)
                        return true;
                }
            }

            return false;
        }

        public InputAction GetAction(KeyAction action)
        {
            if (inputActions == null)
                return null;

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

        public void StartRebind(KeyAction action,Action<string, bool> onComplete)
        {
            InputAction inputAction = GetAction(action);

            if (inputAction == null)
            {
                Debug.LogWarning(
                    $"[KeyBind] Action not found: {action}"
                );

                onComplete?.Invoke("", false);
                return;
            }

            
            string oldBindingPath =
                inputAction.bindings[0].effectivePath;

            inputAction.Disable();

            inputAction.PerformInteractiveRebinding().WithControlsExcluding("<Mouse>/position").OnComplete(operation =>
                {
                    string newBindingPath =
                        inputAction.bindings[0].effectivePath;

                    operation.Dispose();

                    if (IsBindingUsed(
                        newBindingPath,
                        action,
                        0))
                    {
                        inputAction.ApplyBindingOverride(
                            0,
                            oldBindingPath
                        );

                        inputAction.Enable();

                        Debug.LogWarning(
                            $"[KeyBind] {newBindingPath} is already used."
                        );

                        onComplete?.Invoke(
                            newBindingPath,
                            false
                        );

                        return;
                    }

                    inputAction.Enable();

                    onComplete?.Invoke(
                        newBindingPath,
                        true
                    );

                    SaveBindings();

                    OnBindingsChanged?.Invoke();

                    GameLog.Info(
                        $"[KeyBind] {action} → {newBindingPath}"
                    );
                })
                .Start();
        }

        // Move is one composite action; W/A/S/D are rebound as separate parts of it.

        public InputAction GetMoveBinding(MoveDirection direction)
        {
            InputAction moveAction = GetAction(KeyAction.Move);

            if (moveAction == null)
                return null;

            return moveAction;
        }

        public int GetMoveBindingIndex(MoveDirection direction)
        {
            InputAction moveAction = GetAction(KeyAction.Move);

            if (moveAction == null)
                return -1;

            string partName = direction switch
            {
                MoveDirection.Up => "up",
                MoveDirection.Down => "down",
                MoveDirection.Left => "left",
                MoveDirection.Right => "right",
                _ => ""
            };

            for (int i = 0; i < moveAction.bindings.Count; i++)
            {
                var binding = moveAction.bindings[i];

                if (binding.isPartOfComposite &&
                    binding.name == partName)
                {
                    return i;
                }
            }

            return -1;
        }

        public string GetMoveBindingDisplayString(MoveDirection direction)
        {
            InputAction moveAction = GetAction(KeyAction.Move);

            if (moveAction == null)
                return "---";

            int index = GetMoveBindingIndex(direction);

            if (index < 0)
                return "---";

            return moveAction.GetBindingDisplayString(
                index,
                InputBinding.DisplayStringOptions.DontIncludeInteractions
            );
        }

        public void RebindMove(MoveDirection direction,Action<string, bool> onComplete)
        {
            InputAction moveAction =
                GetAction(KeyAction.Move);

            if (moveAction == null)
            {
                Debug.LogWarning(
                    "[KeyBind] Move action not found."
                );

                onComplete?.Invoke("", false);
                return;
            }

            int bindingIndex =
                GetMoveBindingIndex(direction);

            if (bindingIndex < 0)
            {
                Debug.LogWarning(
                    $"[KeyBind] Move binding not found: {direction}"
                );

                onComplete?.Invoke("", false);
                return;
            }

            string oldBindingPath =
                moveAction.bindings[bindingIndex].effectivePath;

            moveAction.Disable();

            moveAction
                .PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(operation =>
                {
                    string newBindingPath =
                        moveAction.bindings[bindingIndex].effectivePath;

                    operation.Dispose();

                    if (IsBindingUsed(
                        newBindingPath,
                        KeyAction.Move,
                        bindingIndex))
                    {
                        moveAction.ApplyBindingOverride(
                            bindingIndex,
                            oldBindingPath
                        );

                        moveAction.Enable();

                        Debug.LogWarning(
                            $"[KeyBind] {newBindingPath} is already used."
                        );

                        onComplete?.Invoke(
                            newBindingPath,
                            false
                        );

                        return;
                    }

                    moveAction.Enable();

                    SaveBindings();

                    OnBindingsChanged?.Invoke();

                    GameLog.Info(
                        $"[KeyBind] Move {direction} → {newBindingPath}"
                    );

                    onComplete?.Invoke(
                        newBindingPath,
                        true
                    );
                })
                .OnCancel(operation =>
                {
                    operation.Dispose();

                    moveAction.Enable();

                    onComplete?.Invoke("", false);

                    GameLog.Info(
                        $"[KeyBind] Move {direction} rebind cancelled."
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

            GameLog.Info(
                $"[KeyBind] Saved: {SavePath}"
            );
        }

        public void LoadBindings()
        {
            if (!File.Exists(SavePath))
            {
                GameLog.Info(
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

            GameLog.Info("[KeyBind] Bindings loaded.");
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

            GameLog.Info(
                "[KeyBind] All bindings reset to default."
            );
        }

        
    }
}