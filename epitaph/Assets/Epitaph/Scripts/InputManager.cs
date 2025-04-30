using Epitaph.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Epitaph.Scripts
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerLook playerLook;
        [SerializeField] private PlayerJump playerJump;
        [SerializeField] private PlayerCrouch playerCrouch;
        
        private InputSystem_Actions _inputSystemActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        private void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _playerActions = _inputSystemActions.Player;

            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }
            
            if (playerLook == null)
            {
                playerLook = GetComponent<PlayerLook>();
            }
            
            if (playerJump == null)
            {
                playerJump = GetComponent<PlayerJump>();
            }

            if (playerCrouch == null)
            {
                playerCrouch = GetComponent<PlayerCrouch>();
            }
        }
        
        private void OnEnable()
        {
            _playerActions.Enable();
            _playerActions.Jump.performed += OnJumpPerformed;
            _playerActions.Crouch.performed += OnCrouchPerformed;

        }

        private void OnDisable()
        {
            _playerActions.Jump.performed -= OnJumpPerformed;
            _playerActions.Crouch.performed -= OnCrouchPerformed;
            _playerActions.Disable();
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            playerJump.ProcessJump();
        }
        
        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("crouch");
            playerCrouch.OnCrouchPerformed();
        }

        private void Update()
        {
            playerMovement.ProcessMove(_playerActions.Move.ReadValue<Vector2>());
        }
    }
}