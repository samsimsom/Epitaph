using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Epitaph.Scripts
{
    public class PlayerInput : MonoBehaviour, PlayerInputActions.IPlayerActions
    {
        public Vector2 mouseDelta;
        public Vector2 moveInput;
        public bool isMoveInput;
        
        private PlayerInputActions _playerInputActions;
        private PlayerController _playerController;
        
        private void Awake()
        {
            if (_playerInputActions != null) return;
            
            _playerController = GetComponent<PlayerController>();
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.SetCallbacks(this);
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
        #region Player Input Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            isMoveInput = moveInput.magnitude > 0.1f;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            mouseDelta = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _playerController.HandleInteract();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _playerController.GetPlayerCrouch().ToggleCrouch();
            }
            else if (context.canceled)
            {
                
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _playerController.GetPlayerJump().ProcessJump();
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GameTime.Instance.SkipTimeAsync(1f).Forget();
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _playerController.GetPlayerSprint().TryStartSprint();
            }
            else if (context.canceled)
            {
                _playerController.GetPlayerSprint().StopSprint();
            }
        }
        #endregion
        // ---------------------------------------------------------------------------- //
        
    }
}