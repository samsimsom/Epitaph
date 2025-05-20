using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("Components")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        
        private List<PlayerBehaviour> _playerBehaviours = new();
        
        // // Movement Components
        // private PlayerMove _playerMove;
        // private PlayerJump _playerJump;
        // private PlayerCrouch _playerCrouch;
        // private PlayerSprint _playerSprint;
        // private PlayerGravity _playerGravity;
        // private PlayerLook _playerLook;
        // private PlayerHeadBob _playerHeadBob;
        // private PlayerInteraction _playerInteraction;
        //
        // // Health System Components
        // private PlayerCondition _playerCondition;
        
        private void Awake()
        {
            InitializeComponents();
            RegisterEvents();
        }
        
        private void OnEnable()
        {
            for (var i = 0; i < _playerBehaviours.Count; i++)
            {
                _playerBehaviours[i].OnEnable();
            }
        }
        
        private void OnDisable()
        {
            for (var i = 0; i < _playerBehaviours.Count; i++)
            {
                _playerBehaviours[i].OnDisable();
            }
        }

        private void Start()
        {
            for (var i = 0; i < _playerBehaviours.Count; i++)
            {
                _playerBehaviours[i].Start();
            }
        }

        private void Update()
        {
            for (var i = 0; i < _playerBehaviours.Count; i++)
            {
                _playerBehaviours[i].Update();
            }
        }

        private void InitializeComponents()
        {
            // Get required components if not already assigned
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (playerInput == null) playerInput = GetComponent<PlayerInput>();
            
            // Get movement system components
            // _playerMove = GetComponent<PlayerMove>();
            // _playerJump = GetComponent<PlayerJump>();
            // _playerCrouch = GetComponent<PlayerCrouch>();
            // _playerGravity = GetComponent<PlayerGravity>();
            // _playerLook = GetComponent<PlayerLook>();
            // _playerHeadBob = GetComponent<PlayerHeadBob>();
            // _playerInteraction = GetComponent<PlayerInteraction>();
            
            // Get health system components
            // _playerCondition = GetComponent<PlayerCondition>();
            
            // _playerSprint = new PlayerSprint(this, playerMovementData, _playerCondition);
            // _playerBehaviours.Add(_playerSprint);
        }
        
        private void RegisterEvents()
        {
            // Connect component events through controller
            // For example: _playerJump.OnJumpStarted += HandleJumpStarted;
        }
        
        // Input handlers
        public void HandleInteract()
        {
            // if (_playerInteraction != null)
            // {
            //     _playerInteraction.TryInteract();
            // }
        }
        
        // Public interfaces for external access
        // public PlayerCondition GetPlayerCondition() => _playerCondition;
        public PlayerMovementData GetMovementData() => playerMovementData;
        public CharacterController GetCharacterController() => characterController;
        
        // Methods for coordinating between systems
        public void SetMovementEnabled(bool enabled)
        {
            // Enable/disable movement components
            // if (_playerMove != null) _playerMove.enabled = enabled;
            // if (_playerJump != null) _playerJump.enabled = enabled;
            // if (_playerCrouch != null) _playerCrouch.enabled = enabled;
            // if (_playerSprint != null) _playerSprint.enabled = enabled;
        }
        
        // Methods for proxying state information
        public bool IsSprinting() => playerMovementData.isSprinting;
        public bool IsCrouching() => playerMovementData.isCrouching;
        public bool IsGrounded() => playerMovementData.isGrounded;
        
        // Interact with specific components
        public void StartSprint()
        {
            // if (_playerSprint != null && _playerCondition?.Stamina != null && _playerCondition.Stamina.Value > 0)
            // {
            //     _playerSprint.TryStartSprint();
            // }
        }
        
        public void StopSprint()
        {
            // if (_playerSprint != null)
            // {
            //     _playerSprint.StopSprint();
            // }
        }
        
        public void ToggleCrouch()
        {
            // if (_playerCrouch != null)
            // {
            //     _playerCrouch.ToggleCrouch();
            // }
        }

        public void Jump()
        {
            // if (_playerJump != null)
            // {
            //     _playerJump.ProcessJump();
            // }
        }
    }
}