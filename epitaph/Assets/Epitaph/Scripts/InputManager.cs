using Epitaph.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Epitaph.Scripts
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private PlayerLook playerLook;
        [SerializeField] private PlayerJump playerJump;
        [SerializeField] private PlayerCrouch playerCrouch;
        [SerializeField] private PlayerSprint playerSprint;
        [SerializeField] private PlayerInteraction playerInteraction;
        
        private InputSystem_Actions _inputSystemActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        private void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _playerActions = _inputSystemActions.Player;

            if (playerMove == null)
            {
                playerMove = GetComponent<PlayerMove>();
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
            
            if (playerSprint == null)
            {
                playerSprint = GetComponent<PlayerSprint>();
            }
            
            if (playerInteraction == null)
            {
                playerInteraction = GetComponent<PlayerInteraction>();
            }
        }
        
        private void OnEnable()
        {
            _playerActions.Enable();
            _playerActions.Jump.performed += OnJumpPerformed;
            _playerActions.Crouch.performed += OnCrouchPerformed;
            
            _playerActions.Sprint.performed += OnSprintPerformed;
            _playerActions.Sprint.canceled += OnSprintPerformed;
            
            // Add interaction input
            _playerActions.Interact.performed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            _playerActions.Jump.performed -= OnJumpPerformed;
            _playerActions.Crouch.performed -= OnCrouchPerformed;
            
            _playerActions.Sprint.performed -= OnSprintPerformed;
            _playerActions.Sprint.canceled -= OnSprintPerformed;
            
            // Remove interaction input
            _playerActions.Interact.performed -= OnInteractPerformed;
            
            _playerActions.Disable();
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            playerJump.ProcessJump();
        }
        
        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            playerCrouch.OnCrouchPerformed();
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            playerSprint.OnSprintPerformed(context);
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            playerInteraction.ProcessInteraction();
        }

        private void Update()
        {
            playerMove.ProcessMove(_playerActions.Move.ReadValue<Vector2>());
        }
    }
}