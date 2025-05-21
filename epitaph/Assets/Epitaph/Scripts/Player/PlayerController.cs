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
        #region Inspector Fields
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CinemachineCamera fpCamera;
        [SerializeField] private Transform playerCameraTransform;
        #endregion
        
        #region Player Behaviors
        private readonly List<PlayerBehaviour> _playerBehaviours = new();
        
        // Movement Components
        private PlayerMove _playerMove;
        private PlayerJump _playerJump;
        private PlayerCrouch _playerCrouch;
        private PlayerSprint _playerSprint;
        private PlayerGravity _playerGravity;
        private PlayerLook _playerLook;
        private PlayerHeadBob _playerHeadBob;
        private PlayerInteraction _playerInteraction;
        
        // Health System Components
        private PlayerCondition _playerCondition;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.Start();
            }
        }
        
        private void Update()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.Update();
            }
        }
        
        private void OnEnable()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.OnEnable();
            }
        }
        
        private void OnDisable()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.OnDisable();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.OnDrawGizmos();
            }
        }
#endif
        #endregion

        #region Initialization
        private T AddPlayerBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _playerBehaviours.Add(behaviour);
            return behaviour;
        }
        
        private void InitializeComponents()
        {
            // Get required components if not already assigned
            if (characterController == null) 
                characterController = GetComponent<CharacterController>();

            if (playerInput == null) 
                playerInput = GetComponent<PlayerInput>();

            // Initialize player condition first as other components may depend on it
            _playerCondition = AddPlayerBehaviour(new PlayerCondition(this, playerData));
            
            // Initialize movement components
            _playerLook = AddPlayerBehaviour(new PlayerLook(this, playerData, playerCamera, fpCamera));
            _playerMove = AddPlayerBehaviour(new PlayerMove(this, playerData, characterController, playerCamera));
            _playerGravity = AddPlayerBehaviour(new PlayerGravity(this, playerData, characterController));
            _playerSprint = AddPlayerBehaviour(new PlayerSprint(this, playerData, _playerCondition, _playerMove));
            _playerCrouch = AddPlayerBehaviour(new PlayerCrouch(this, playerData, characterController, _playerMove, playerCamera));
            _playerJump = AddPlayerBehaviour(new PlayerJump(this, playerData));
            _playerHeadBob = AddPlayerBehaviour(new PlayerHeadBob(this, playerData, playerCameraTransform));
            _playerInteraction = AddPlayerBehaviour(new PlayerInteraction(this, playerData, playerCamera));
        }
        #endregion
        
        #region Public Accessor Methods
        // Public interfaces for external access
        public CharacterController GetCharacterController() => characterController;
        public PlayerData GetMovementData() => playerData;
        public PlayerInput GetPlayerInput() => playerInput;
        
        // Player component accessors
        public PlayerCondition GetPlayerCondition() => _playerCondition;
        public PlayerLook GetPlayerLook() => _playerLook;
        public PlayerMove GetPlayerMove() => _playerMove;
        public PlayerGravity GetPlayerGravity() => _playerGravity;
        public PlayerSprint GetPlayerSprint() => _playerSprint;
        public PlayerCrouch GetPlayerCrouch() => _playerCrouch;
        public PlayerJump GetPlayerJump() => _playerJump;
        public PlayerHeadBob GetPlayerHeadBob() => _playerHeadBob;
        public PlayerInteraction GetPlayerInteraction() => _playerInteraction;
        #endregion
        
        #region State Methods
        // Methods for proxying state information
        public bool IsSprinting() => playerData.isSprinting;
        public bool IsCrouching() => playerData.isCrouching;
        public bool IsGrounded() => playerData.isGrounded;
        #endregion
    }
}