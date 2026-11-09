using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Core
{
    public class KeyBindManager : Singleton<KeyBindManager>
    {

        public static event Action OnInitialized;

        public static event Action OnBindingsChanged;

        /// <summary>True while a rebind waits for input; UI back/submit must ignore presses meanwhile.</summary>
        public static bool IsRebinding { get; private set; }

        /// <summary>Frame a rebind started or ended; the press that ends it is still live that frame.</summary>
        public static int LastRebindFrame { get; private set; } = -1;

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

        public InputAction GetAction(KeyAction action)
        {
            if (inputActions == null)
                return null;

            return action switch
            {
                KeyAction.Move => inputActions.Player.Move,
                KeyAction.Interact => inputActions.Player.Interact,
                KeyAction.Inventory => inputActions.Player.Inventory,
                KeyAction.UseItem => inputActions.Player.ItemConsume,
                KeyAction.Sprint => inputActions.Player.Sprint,
                KeyAction.Attack => inputActions.Player.Attack,
                KeyAction.Aim => inputActions.Player.Aim,
                _ => null
            };
        }

        /// <summary>Index of the action's plain (non-composite) binding in a scheme, or -1.</summary>
        public static int GetBindingIndex(InputAction action, ControlScheme scheme)
        {
            if (action == null)
                return -1;

            InputBinding mask = InputBinding.MaskByGroup(scheme.Group());
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (!binding.isComposite && !binding.isPartOfComposite && mask.Matches(binding))
                    return i;
            }
            return -1;
        }

        public string GetDisplayString(KeyAction action, ControlScheme scheme)
        {
            InputAction inputAction = GetAction(action);
            int index = GetBindingIndex(inputAction, scheme);
            return index < 0 ? "---" : DisplayString(inputAction, index);
        }

        // Gamepad bindings are generic (<Gamepad>/buttonSouth); name them after the connected pad ("Cross", "A").
        private static string DisplayString(InputAction action, int index)
        {
            // Live keyboard names follow the OS layout (Thai layout turns W into "ไ", which our font lacks); the layout's own names are fixed English.
            string path = action.bindings[index].effectivePath;
            if (InputControlPath.TryGetDeviceLayout(path) == "Keyboard")
                return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);

            string text = action.GetBindingDisplayString(index, out string layout, out string controlPath,
                InputBinding.DisplayStringOptions.DontIncludeInteractions);

            Gamepad pad = Gamepad.current;
            if (pad == null || string.IsNullOrEmpty(layout) || string.IsNullOrEmpty(controlPath)
                || !InputSystem.IsFirstLayoutBasedOnSecond(pad.layout, layout))
                return text;

            InputControl control = pad.TryGetChildControl(controlPath);
            if (control == null)
                return text;
            return !string.IsNullOrEmpty(control.shortDisplayName) ? control.shortDisplayName : control.displayName;
        }

        public bool IsBindingUsed(string bindingPath, InputAction excludeAction, int excludeBindingIndex)
        {
            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                InputAction inputAction = GetAction(action);
                if (inputAction == null)
                    continue;

                for (int i = 0; i < inputAction.bindings.Count; i++)
                {
                    if (inputAction == excludeAction && i == excludeBindingIndex)
                        continue;

                    if (inputAction.bindings[i].effectivePath == bindingPath)
                        return true;
                }
            }

            return false;
        }

        public void StartRebind(KeyAction action, ControlScheme scheme, Action<string, bool> onComplete)
        {
            InputAction inputAction = GetAction(action);
            int index = GetBindingIndex(inputAction, scheme);

            if (index < 0)
            {
                Debug.LogWarning($"[KeyBind] No {scheme} binding for {action}");
                onComplete?.Invoke("", false);
                return;
            }

            Rebind(inputAction, index, scheme, $"{action}", onComplete);
        }

        // Move is one composite action; W/A/S/D are rebound as separate parts of it. Keyboard only:
        // the gamepad moves with a stick, which can't be split into per-direction keys.

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

            InputBinding mask = InputBinding.MaskByGroup(ControlScheme.KeyboardMouse.Group());
            for (int i = 0; i < moveAction.bindings.Count; i++)
            {
                var binding = moveAction.bindings[i];

                if (binding.isPartOfComposite && binding.name == partName && mask.Matches(binding))
                    return i;
            }

            return -1;
        }

        public string GetMoveBindingDisplayString(MoveDirection direction)
        {
            InputAction moveAction = GetAction(KeyAction.Move);
            int index = GetMoveBindingIndex(direction);
            return index < 0 ? "---" : DisplayString(moveAction, index);
        }

        public void RebindMove(MoveDirection direction, Action<string, bool> onComplete)
        {
            InputAction moveAction = GetAction(KeyAction.Move);
            int bindingIndex = GetMoveBindingIndex(direction);

            if (bindingIndex < 0)
            {
                Debug.LogWarning($"[KeyBind] Move binding not found: {direction}");
                onComplete?.Invoke("", false);
                return;
            }

            Rebind(moveAction, bindingIndex, ControlScheme.KeyboardMouse, $"Move {direction}", onComplete);
        }

        private void Rebind(InputAction action, int index, ControlScheme scheme, string label, Action<string, bool> onComplete)
        {
            string oldOverride = action.bindings[index].overridePath;

            action.Disable();
            SetRebinding(true);

            var operation = action.PerformInteractiveRebinding(index)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                // Picks the strongest control pressed together, and lets the press settle before UI input resumes.
                .OnMatchWaitForAnother(0.1f);

            // A press from the other device family must never land in this scheme's slot.
            if (scheme == ControlScheme.Gamepad)
            {
                operation
                    .WithControlsHavingToMatchPath("<Gamepad>")
                    .WithControlsExcluding("<Gamepad>/leftStick")
                    .WithControlsExcluding("<Gamepad>/rightStick")
                    .WithCancelingThrough("<Gamepad>/start");
            }
            else
            {
                operation
                    .WithControlsHavingToMatchPath("<Keyboard>")
                    .WithControlsHavingToMatchPath("<Mouse>")
                    .WithCancelingThrough("<Keyboard>/escape");
            }

            operation
                .OnComplete(op =>
                {
                    op.Dispose();
                    SetRebinding(false);

                    string newBindingPath = action.bindings[index].effectivePath;

                    if (IsBindingUsed(newBindingPath, action, index))
                    {
                        if (string.IsNullOrEmpty(oldOverride))
                            action.RemoveBindingOverride(index);
                        else
                            action.ApplyBindingOverride(index, oldOverride);

                        action.Enable();
                        Debug.LogWarning($"[KeyBind] {newBindingPath} is already used.");
                        onComplete?.Invoke(newBindingPath, false);
                        return;
                    }

                    action.Enable();
                    SaveBindings();
                    OnBindingsChanged?.Invoke();
                    GameLog.Info($"[KeyBind] {label} ({scheme}) → {newBindingPath}");
                    onComplete?.Invoke(newBindingPath, true);
                })
                .OnCancel(op =>
                {
                    op.Dispose();
                    SetRebinding(false);
                    action.Enable();
                    onComplete?.Invoke("", false);
                    GameLog.Info($"[KeyBind] {label} rebind cancelled.");
                })
                .Start();
        }

        // While waiting, the pressed gamepad button must not also navigate or click the settings UI.
        private static void SetRebinding(bool rebinding)
        {
            IsRebinding = rebinding;
            LastRebindFrame = Time.frameCount;
            if (EventSystem.current != null)
                EventSystem.current.sendNavigationEvents = !rebinding;
        }

        // Save Key Bind -----------------------------------------------
        // Overrides are stored by binding id, so adding bindings to the asset never shifts saved keys.
        public void SaveBindings()
        {
            File.WriteAllText(SavePath, inputActions.asset.SaveBindingOverridesAsJson());
            GameLog.Info($"[KeyBind] Saved: {SavePath}");
        }

        public void LoadBindings()
        {
            if (!File.Exists(SavePath))
            {
                GameLog.Info("[KeyBind] No saved bindings found. Using defaults.");
                return;
            }

            string json = File.ReadAllText(SavePath);

            // Saving with no overrides (e.g. right after a reset) writes an empty file.
            if (string.IsNullOrWhiteSpace(json))
                return;

            // ponytail: the old index-based file ({"bindings":[{"bindingPaths":..}]}) is ignored, not migrated; add a migration if players already shipped with it.
            if (json.Contains("\"bindingPaths\""))
            {
                GameLog.Info("[KeyBind] Old keybind file format; using defaults.");
                return;
            }

            inputActions.asset.LoadBindingOverridesFromJson(json);
            GameLog.Info("[KeyBind] Bindings loaded.");
        }

        /// <summary>Restores the default bindings of one scheme, leaving the other scheme's rebinds alone.</summary>
        public void ResetScheme(ControlScheme scheme)
        {
            InputBinding mask = InputBinding.MaskByGroup(scheme.Group());

            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                InputAction inputAction = GetAction(action);
                if (inputAction == null)
                    continue;

                for (int i = 0; i < inputAction.bindings.Count; i++)
                {
                    if (mask.Matches(inputAction.bindings[i]))
                        inputAction.RemoveBindingOverride(i);
                }
            }

            SaveBindings();
            OnBindingsChanged?.Invoke();
            GameLog.Info($"[KeyBind] {scheme} bindings reset to default.");
        }
    }
}
