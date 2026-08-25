using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
    public class KeyBindManager : MonoBehaviour
    {
        public static KeyBindManager Instance { get; private set; }

        public static event Action OnInitialized;

        private InputSystem_Actions inputActions;

        public InputSystem_Actions Actions => inputActions;

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

            OnInitialized?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            inputActions?.Dispose();
            Instance = null;
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
            Action<string> onComplete)
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

                    inputAction.Enable();

                    onComplete?.Invoke(bindingPath);

                    Debug.Log(
                        $"[KeyBind] {action} → {bindingPath}"
                    );
                })
                .Start();
        }
    }
}