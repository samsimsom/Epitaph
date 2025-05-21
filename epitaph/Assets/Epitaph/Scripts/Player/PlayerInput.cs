using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Epitaph.Scripts.Player
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
            // PlayerLook'a mouse deltasını ilet
            _playerController.ViewController?.PlayerLook?.SetMouseInput(mouseDelta); // PlayerLook'ta böyle bir metot olduğunu varsayıyoruz
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            // Henüz bir AttackController veya benzeri bir yapı yok,
            // bu implemente edildiğinde ilgili kontrolcü üzerinden çağrılacak.
            // Örneğin: _playerController.AttackController?.PlayerAttack?.TryAttack();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // Eski: _playerController.GetPlayerInteraction().TryInteract();
                // Yeni:
                _playerController.InteractionController?.PlayerInteraction?.TryInteract();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                // Eski: _playerController.GetPlayerCrouch().ToggleCrouch();
                // Yeni:
                _playerController.MovementController?.PlayerCrouch?.ToggleCrouch();
            }
            else if (context.canceled)
            {
                // Gerekirse burası da güncellenir.
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // Eski: _playerController.GetPlayerJump().ProcessJump();
                // Yeni:
                _playerController.MovementController?.PlayerJump?.ProcessJump();
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
                // Eski: _playerController.GetPlayerSprint().TryStartSprint();
                // Yeni:
                _playerController.MovementController?.PlayerSprint?.TryStartSprint();
            }
            else if (context.canceled)
            {
                // Eski: _playerController.GetPlayerSprint().StopSprint();
                // Yeni:
                _playerController.MovementController?.PlayerSprint?.StopSprint();
            }
        }
        #endregion
        // ---------------------------------------------------------------------------- //
    }
}