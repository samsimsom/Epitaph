using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementController : PlayerBehaviour
    {
        private readonly List<PlayerBehaviour> _movementBehaviours = new();
        
        private CharacterController _characterController;
        private HealthController _healthController;
        private Camera _playerCamera;
        private Transform _playerCameraTransform;
        private PlayerController _playerController;

        private PlayerData _playerData;

        public MovementController(PlayerController playerController, 
            PlayerData playerData,
            CharacterController characterController, 
            Transform playerCameraTransform,
            HealthController healthController,
            Camera playerCamera) 
            : base(playerController)
        {
            _playerController = playerController;
            _playerData = playerData;
            _characterController = characterController;
            _playerCamera = playerCamera;
            _playerCameraTransform = playerCameraTransform;
            _healthController = healthController;
        }
        
        public PlayerMove PlayerMove { get; private set; }
        public PlayerJump PlayerJump { get; private set; }
        public PlayerCrouch PlayerCrouch { get; private set; }
        public PlayerSprint PlayerSprint { get; private set; }
        public PlayerGravity PlayerGravity { get; private set; }

        private T AddMovementBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _movementBehaviours.Add(behaviour);
            return behaviour;
        }

        public void InitializeBehaviours()
        {
            PlayerMove = AddMovementBehaviour(new PlayerMove(_playerController, _playerData, _characterController, _playerCamera));
            // PlayerSprint = AddMovementBehaviour(new PlayerSprint(_playerController, _playerData, _healthController, PlayerMove)); // HealthController ve PlayerMove bağımlılığı
            // PlayerCrouch = AddMovementBehaviour(new PlayerCrouch(_playerController, _playerData, _characterController, PlayerMove, _playerCameraTransform)); // PlayerMove bağımlılığı
            // PlayerJump = AddMovementBehaviour(new PlayerJump(_playerController, _playerData));
            // PlayerGravity = AddMovementBehaviour(new PlayerGravity(_playerController, _playerData, _characterController));
        }

        #region MonoBehaviour Methods

        public override void Awake()
        {
            InitializeBehaviours();
            foreach (var behaviour in _movementBehaviours) behaviour.Awake();
        }

        public override void OnEnable()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnEnable();
        }

        public override void Start()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.Start();
        }

        public override void Update()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.Update();
        }

        public override void LateUpdate()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.LateUpdate();
        }

        public override void FixedUpdate()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.FixedUpdate();
        }

        public override void OnDisable()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnDisable();
        }

        public override void OnDestroy()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnDestroy();
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnDrawGizmos();
        }
#endif

        #endregion
        
    }
}