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
        public event Action<bool> OnDashed;
        public event Action<bool> OnInteracted;
        public event Action<bool> OnAmmoSelected;
        public event Action<Vector2> OnMoved;

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

        public void EnablePlayerInput() => inputAction.Player.Enable();

        public void DisablePlayerInput() => inputAction.Player.Disable();

        public void EnableUIInput() => inputAction.UI.Enable();

        public void DisableUIInput() => inputAction.UI.Disable();

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnAimed?.Invoke(true);
            }
            else if (context.canceled)
            {
                OnAimed?.Invoke(false);
            }
        }
        public void OnMousePos(InputAction.CallbackContext context)
        {
            MousePos = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnAttacked?.Invoke(true);
            }
            else if (context.canceled)
            {
                OnAttacked?.Invoke(false);
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnDashed?.Invoke(true);
            }
            else if (context.canceled)
            {
                OnDashed?.Invoke(false);
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnInteracted?.Invoke(true);
            }
            else if (context.canceled)
            {
                OnInteracted?.Invoke(false);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MovePos = context.ReadValue<Vector2>();
            OnMoved?.Invoke(MovePos);
        }

        public void OnAmmoWheel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnAmmoSelected?.Invoke(true);
            }
            else if (context.canceled)
            {
                OnAmmoSelected?.Invoke(false);
            }
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }
    }
}