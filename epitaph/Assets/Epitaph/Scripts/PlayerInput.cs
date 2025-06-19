using UnityEngine;
using UnityEngine.InputSystem;

namespace Epitaph.Scripts
{
    public class PlayerInput : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        public Vector2 MoveInput { get; private set; }
        public bool IsMoveInput { get; private set; }
        
        public Vector2 LookInput { get; private set; }
        public bool IsLookInput { get; private set; }
        
        public bool IsJumpPressed { get; private set; }
        public bool IsJumpPressedThisFrame { get; private set; }

        public bool IsCrouchPressed { get; private set; }
        public bool IsCrouchPressedThisFrame { get; private set; }

        public bool IsRunPressed { get; private set; }
        
        private InputSystem_Actions _playerInputActions;

        private void Awake()
        {
            if (_playerInputActions != null) return;
            _playerInputActions = new InputSystem_Actions();
            _playerInputActions.Player.SetCallbacks(this);
        }

        private void LateUpdate()
        {
            IsJumpPressedThisFrame = false;
            IsCrouchPressedThisFrame = false;
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Disable();
        }
        
        // ---------------------------------------------------------------------------- //
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveInput = context.ReadValue<Vector2>();
                IsMoveInput = MoveInput.magnitude != 0;
            }
            else if (context.canceled)
            {
                MoveInput = Vector2.zero;
                IsMoveInput = false;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                LookInput = context.ReadValue<Vector2>();
                IsLookInput = LookInput.magnitude != 0;
            }
            else if (context.canceled)
            {
                LookInput = Vector2.zero;
                IsLookInput = false;
            }
        }

        public void OnAttack(InputAction.CallbackContext context) { }

        public void OnInteract(InputAction.CallbackContext context) { }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed) // Toggle için sadece performed kullanıyoruz
            {
                IsCrouchPressedThisFrame = true;
                IsCrouchPressed = !IsCrouchPressed; // Toggle işlemi
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                IsJumpPressedThisFrame = true;
                IsJumpPressed = true;
            }
            else if (context.canceled)
            {
                IsJumpPressed = false;
            }
        }

        public void OnPrevious(InputAction.CallbackContext context) { }

        public void OnNext(InputAction.CallbackContext context) { }

        public void OnSprint(InputAction.CallbackContext context) { }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                IsRunPressed = true;
            }
            else if (context.canceled)
            {
                IsRunPressed = false;
            }
        }
        
        // ---------------------------------------------------------------------------- //
    }
}