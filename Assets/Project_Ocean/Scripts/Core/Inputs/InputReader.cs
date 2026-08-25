using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/InputReader")]
    public class InputReader : ScriptableObject, IPlayerActions, IUIActions
    {
        public event Action<bool> OnAimed;
        public event Action<bool> OnAttacked;
        public event Action<bool> OnSprinted;
        public event Action<bool> OnInteracted;
        public event Action<bool> OnAmmoSelected;
        public event Action<bool> OnInventoryToggled;
        public event Action<Vector2> OnMoved;

        public event Action OnUseItem;
        public event Action OnTeleport;

        public Vector2 MousePos { get; private set; }
        public Vector2 MovePos { get; private set; }

        private InputSystem_Actions inputAction;

        private void OnEnable()
        {
            if (inputAction == null)
            {
                inputAction = new InputSystem_Actions();
                inputAction.Player.SetCallbacks(this);
            }

            EnablePlayerInput();
        }

        private void OnDisable()
        {
            DisablePlayerInput();
        }

        public void EnablePlayerInput()  => inputAction?.Player.Enable();
        public void DisablePlayerInput() => inputAction?.Player.Disable();
        public void EnableUIInput()      => inputAction?.UI.Enable();
        public void DisableUIInput()     => inputAction?.UI.Disable();

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

        /// <summary>
        /// Simulates pressing the Interact input. Wire this to an on-screen UI Button's
        /// OnClick() so tapping it goes through the exact same locking path as pressing E
        /// (whichever InteractUI/ChestInteract/etc. the player is currently in range of).
        /// </summary>
        public void TriggerInteract() => OnInteracted?.Invoke(true);

        public void OnItemConsume(InputAction.CallbackContext context)
        {
            if (context.performed) OnUseItem?.Invoke();
        }

        void IPlayerActions.OnTeleport(InputAction.CallbackContext context)
        {
            if (context.performed) OnTeleport?.Invoke();
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

        // ── IUIActions ─────────────────────────────────────────────────────

        public void OnSubmit(InputAction.CallbackContext context) { }
        public void OnCancel(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnMiddleClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }
    }
}
