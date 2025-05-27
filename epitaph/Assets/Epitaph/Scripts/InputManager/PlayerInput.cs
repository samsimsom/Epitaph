using Epitaph.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Epitaph.Scripts.InputManager
{
    public class PlayerInput : MonoBehaviour, PlayerInputActions.IPlayerActions
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
        
        private PlayerInputActions _playerInputActions;
        private PlayerController _playerController;

        private void Awake()
        {
            if (_playerInputActions != null) return;
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.SetCallbacks(this);
            
            _playerController = GetComponent<PlayerController>();
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

        public void OnAttack(InputAction.CallbackContext context)
        {
            
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _playerController.InteractionBehaviour.TryInteract();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                IsCrouchPressedThisFrame = true;
                IsCrouchPressed = true;
            }
            else if (context.canceled)
            {
                IsCrouchPressed = false;
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

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _playerController.LifeStatsManager.AddStat("Thirst", -10f);
                _playerController.LifeStatsManager.AddStat("Hunger", -10f);
                _playerController.LifeStatsManager.AddStat("Fatique", -10f);
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            
        }

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