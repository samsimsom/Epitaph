using System;
using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CinemachineCamera fpCamera;
        [SerializeField] private Transform playerCameraTransform;
        
        private List<PlayerBehaviour> _playerBehaviours = new();
        
        // // Movement Components
        private PlayerMove _playerMove;
        private PlayerJump _playerJump;
        private PlayerCrouch _playerCrouch;
        private PlayerSprint _playerSprint;
        private PlayerGravity _playerGravity;
        private PlayerLook _playerLook;
        private PlayerHeadBob _playerHeadBob;
        private PlayerInteraction _playerInteraction;
        // // Health System Components
        private PlayerCondition _playerCondition;
        
        private void Awake()
        {
            InitializeComponents();
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

            _playerCondition = new PlayerCondition(this, playerData);
            
            _playerLook = new PlayerLook(this, 
                playerData, playerCamera, fpCamera);
            
            _playerMove = new PlayerMove(this,
                playerData, characterController, playerCamera);

            _playerSprint = new PlayerSprint(this, playerData, 
                _playerCondition);

            _playerGravity = new PlayerGravity(this, playerData, 
                characterController);

            _playerCrouch = new PlayerCrouch(this, playerData, 
                characterController, playerCamera);

            _playerJump = new PlayerJump(this, playerData);

            _playerHeadBob = new PlayerHeadBob(this, playerData, 
                playerCameraTransform);

            _playerInteraction = new PlayerInteraction(this, playerCamera);
            
            _playerBehaviours.Add(_playerCondition);
            _playerBehaviours.Add(_playerLook);
            _playerBehaviours.Add(_playerMove);
            _playerBehaviours.Add(_playerGravity);
            _playerBehaviours.Add(_playerSprint);
            _playerBehaviours.Add(_playerCrouch);
            _playerBehaviours.Add(_playerJump);
            _playerBehaviours.Add(_playerHeadBob);
            _playerBehaviours.Add(_playerInteraction);
        }
        
        // Public interfaces for external access
        public CharacterController GetCharacterController() => characterController;
        public PlayerData GetMovementData() => playerData;
        public PlayerInput GetPlayerInput() => playerInput;
        public PlayerLook GetPlayerLook() => _playerLook;
        public PlayerSprint GetPlayerSprint() => _playerSprint;
        public PlayerCondition GetPlayerCondition() => _playerCondition;
        public PlayerGravity GetPlayerGravity() => _playerGravity;
        public PlayerCrouch GetPlayerCrouch() => _playerCrouch;
        public PlayerJump GetPlayerJump() => _playerJump;
        public PlayerMove GetPlayerMove() => _playerMove;
        public PlayerHeadBob GetPlayerHeadBob() => _playerHeadBob;
        public PlayerInteraction GetPlayerInteraction() => _playerInteraction;
        
        
        // Methods for proxying state information
        public bool IsSprinting() => playerData.isSprinting;
        public bool IsCrouching() => playerData.isCrouching;
        public bool IsGrounded() => playerData.isGrounded;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (var i = 0; i < _playerBehaviours.Count; i++)
            {
                _playerBehaviours[i].OnDrawGizmos();
            }
        }
#endif
        
    }
}