using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementController : IPlayerSubController
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        private CharacterController _characterController;
        private Camera _playerCamera;
        
        // Bağımlı olunan diğer sistemler (HealthController örneği)
        private HealthController _healthController;

        private readonly List<PlayerBehaviour> _movementBehaviours = new();

        // Hareket Davranışları
        public PlayerMove PlayerMove { get; private set; }
        public PlayerJump PlayerJump { get; private set; }
        public PlayerCrouch PlayerCrouch { get; private set; }
        public PlayerSprint PlayerSprint { get; private set; }
        public PlayerGravity PlayerGravity { get; private set; }
        // Diğer hareketle ilgili davranışlar buraya eklenebilir (örn: PlayerSlide)

        public MovementController(CharacterController characterController, Camera playerCamera)
        {
            _characterController = characterController;
            _playerCamera = playerCamera;
        }

        // Bu metot, HealthController gibi diğer sistemlere olan bağımlılıkları set etmek için kullanılabilir.
        public void InjectDependencies(HealthController healthController)
        {
            _healthController = healthController;
        }

        private T AddMovementBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _movementBehaviours.Add(behaviour);
            return behaviour;
        }

        public void InitializeBehaviours(PlayerController playerController, PlayerData playerData)
        {
            _playerController = playerController;
            _playerData = playerData;

            // PlayerMove (diğerlerine bağımlılık oluşturabilir)
            PlayerMove = AddMovementBehaviour(new PlayerMove(_playerController, _playerData, _characterController, _playerCamera));
            
            // Diğer hareket davranışları
            PlayerGravity = AddMovementBehaviour(new PlayerGravity(_playerController, _playerData, _characterController));
            PlayerSprint = AddMovementBehaviour(new PlayerSprint(_playerController, _playerData, _healthController, PlayerMove)); // HealthController ve PlayerMove bağımlılığı
            PlayerCrouch = AddMovementBehaviour(new PlayerCrouch(_playerController, _playerData, _characterController, PlayerMove, _playerCamera)); // PlayerMove bağımlılığı
            PlayerJump = AddMovementBehaviour(new PlayerJump(_playerController, _playerData));
            
            // NOT: PlayerLook ve PlayerHeadBob ViewController'a taşınacak.
            // NOT: PlayerInteraction InteractionController'a taşınacak.
        }

        public void PlayerAwake()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.Awake();
        }

        public void PlayerOnEnable()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnEnable();
        }

        public void PlayerStart()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.Start();
        }

        public void PlayerUpdate()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.Update();
        }

        public void PlayerLateUpdate()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.LateUpdate();
        }

        public void PlayerFixedUpdate()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.FixedUpdate();
        }

        public void PlayerOnDisable()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnDisable();
        }

        public void PlayerOnDestroy()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnDestroy();
        }
        
#if UNITY_EDITOR
        public void PlayerOnDrawGizmos()
        {
            foreach (var behaviour in _movementBehaviours) behaviour.OnDrawGizmos();
        }
#endif
    }
}