using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CinemachineCamera fpCamera;
        
        private List<PlayerBehaviour> _playerBehaviours = new();
        
        // // Movement Components
        private PlayerMove _playerMove;
        private PlayerJump _playerJump;
        private PlayerCrouch _playerCrouch;
        private PlayerSprint _playerSprint;
        private PlayerGravity _playerGravity;
        private PlayerLook _playerLook;
        // private PlayerHeadBob _playerHeadBob;
        // private PlayerInteraction _playerInteraction;
        
        // // Health System Components
        private PlayerCondition _playerCondition;
        
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
            if (characterController == null) 
                characterController = GetComponent<CharacterController>();

            if (playerInput == null) 
                playerInput = GetComponent<PlayerInput>();

            _playerCondition = new PlayerCondition(this);
            
            _playerLook = new PlayerLook(this, 
                playerMovementData, playerCamera, fpCamera);
            
            _playerMove = new PlayerMove(this,
                playerMovementData, characterController);

            _playerSprint = new PlayerSprint(this, playerMovementData, 
                _playerCondition);

            _playerGravity = new PlayerGravity(this, playerMovementData, 
                characterController);

            _playerCrouch = new PlayerCrouch(this, playerMovementData, 
                characterController, playerCamera);

            _playerJump = new PlayerJump(this, playerMovementData);
            
            _playerBehaviours.Add(_playerCondition);
            _playerBehaviours.Add(_playerLook);
            _playerBehaviours.Add(_playerMove);
            _playerBehaviours.Add(_playerGravity);
            _playerBehaviours.Add(_playerSprint);
            _playerBehaviours.Add(_playerCrouch);
            _playerBehaviours.Add(_playerLook);
            _playerBehaviours.Add(_playerJump);
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
        public CharacterController GetCharacterController() => characterController;
        public PlayerMovementData GetMovementData() => playerMovementData;
        public PlayerInput GetPlayerInput() => playerInput;
        public PlayerLook GetPlayerLook() => _playerLook;
        public PlayerSprint GetPlayerSprint() => _playerSprint;
        public PlayerCondition GetPlayerCondition() => _playerCondition;
        public PlayerGravity GetPlayerGravity() => _playerGravity;
        public PlayerCrouch GetPlayerCrouch() => _playerCrouch;
        public PlayerJump GetPlayerJump() => _playerJump;
        
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
            if (_playerSprint != null && _playerCondition?.Stamina != null && _playerCondition.Stamina.Value > 0)
            {
                _playerSprint.TryStartSprint();
            }
        }
        
        public void StopSprint()
        {
            if (_playerSprint != null)
            {
                _playerSprint.StopSprint();
            }
        }
        
        // public void ToggleCrouch()
        // {
        //     if (_playerCrouch != null)
        //     {
        //         _playerCrouch.ToggleCrouch();
        //     }
        // }

        public void Jump()
        {
            // if (_playerJump != null)
            // {
            //     _playerJump.ProcessJump();
            // }
        }
    }
}