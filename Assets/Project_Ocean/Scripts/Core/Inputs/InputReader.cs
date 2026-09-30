using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/InputReader")]
    public class InputReader : ScriptableObject, IPlayerActions, IUIActions
    {
        /// <summary>Set by DialogueState (other assembly) so a left click can advance dialogue.</summary>
        public static Func<bool> IsDialogueShowing;

        public event Action<bool> OnAimed;
        public event Action<bool> OnAttacked;
        public event Action<bool> OnSprinted;
        public event Action<bool> OnInteracted;
        public event Action<bool> OnAmmoSelected;
        public event Action<bool> OnInventoryToggled;
        public event Action OnMenuToggled;
        public event Action<Vector2> OnMoved;

        public event Action OnUseItem;
        public event Action OnTeleport;
        public event Action OnSkipCutscene;

        public Vector2 MousePos { get; private set; }
        public Vector2 MovePos { get; private set; }

        private InputSystem_Actions InputActions =>
          KeyBindManager.Instance != null
        ? KeyBindManager.Instance.Actions
        : null;

        private bool isRegistered;

        private void OnEnable()
        {
            if (KeyBindManager.Instance == null)
            {
                KeyBindManager.OnInitialized += HandleKeyBindManagerInitialized;
                return;
            }

            InitializeInput();
        }

        private void OnDisable()
        {
            KeyBindManager.OnInitialized -= HandleKeyBindManagerInitialized;

            if (!isRegistered)
                return;

            DisablePlayerInput();

            isRegistered = false;
        }

        public void EnablePlayerInput()
        {
            InputActions?.Player.Enable();
        }

        public void DisablePlayerInput()
        {
            InputActions?.Player.Disable();
        }

        public void EnableUIInput()
        {
            InputActions?.UI.Enable();
        }

        public void DisableUIInput()
        {
            InputActions?.UI.Disable();
        }

        // ── IPlayerActions ────────────────────────────────────────────────

        public void OnMove(InputAction.CallbackContext context)
        {
            MovePos = context.ReadValue<Vector2>();
            OnMoved?.Invoke(MovePos);
        }

        public void OnMousePos(InputAction.CallbackContext context)
        {
            MousePos = context.ReadValue<Vector2>();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed) OnAimed?.Invoke(true);
            else if (context.canceled) OnAimed?.Invoke(false);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            // Left click advances dialogue like Interact does, instead of attacking.
            if (context.performed && IsDialogueShowing != null && IsDialogueShowing())
            {
                TriggerInteract();
                return;
            }

            if (context.performed) OnAttacked?.Invoke(true);
            else if (context.canceled) OnAttacked?.Invoke(false);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed) OnSprinted?.Invoke(true);
            else if (context.canceled) OnSprinted?.Invoke(false);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started) OnInteracted?.Invoke(true);
            else if (context.canceled) OnInteracted?.Invoke(false);
        }

        /// <summary>Presses Interact from an on-screen button, through the same path as the E key.</summary>
        public void TriggerInteract() => OnInteracted?.Invoke(true);

        public void OnItemConsume(InputAction.CallbackContext context)
        {
            if (context.performed) OnUseItem?.Invoke();
        }

        void IPlayerActions.OnTeleport(InputAction.CallbackContext context)
        {
            if (context.performed) OnTeleport?.Invoke();
        }

        void IPlayerActions.OnSkipCutscene(InputAction.CallbackContext context)
        {
            if (context.performed) OnSkipCutscene?.Invoke();
        }

        public void OnSwitchItem(InputAction.CallbackContext context)
        {
            if (context.performed) OnAmmoSelected?.Invoke(true);
            else if (context.canceled) OnAmmoSelected?.Invoke(false);
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (context.performed) OnInventoryToggled?.Invoke(true);
            else if (context.canceled) OnInventoryToggled?.Invoke(false);
        }

        public void OnMenu(InputAction.CallbackContext context)
        {
            if (context.performed) OnMenuToggled?.Invoke();
        }

        // ── IUIActions ─────────────────────────────────────────────────────

        public void OnSubmit(InputAction.CallbackContext context) { }
        public void OnCancel(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnMiddleClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }

        private void HandleKeyBindManagerInitialized()
        {
            KeyBindManager.OnInitialized -= HandleKeyBindManagerInitialized;

            InitializeInput();
        }

        private void InitializeInput()
        {
            if (isRegistered)
                return;

            if (KeyBindManager.Instance == null)
                return;

            InputActions.Player.SetCallbacks(this);

            EnablePlayerInput();

            isRegistered = true;
        }
    }
}
