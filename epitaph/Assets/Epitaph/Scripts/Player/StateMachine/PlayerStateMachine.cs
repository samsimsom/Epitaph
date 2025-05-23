using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerStateMachine : PlayerBehaviour
    {
        // Movement Variables
        public float WalkSpeed = 3.0f;
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 2.0f;
        // private float _rotationSpeed = 720f; // Karakterin dönüş hızı
        public float JumpForce = 5.0f;
        public float Gravity = 20.0f;
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public Vector3 NormalControllerCenter = new Vector3(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new Vector3(0, 0.45f, 0);
        // private Animator _animator; // Opsiyonel, animasyonlar için

        // Referanslar
        private CharacterController _characterController;
        private PlayerInput _playerInput;
        private Vector2 _currentMovementInput;
        private Camera _playerCamera;
        private PlayerData _playerData;
        private PlayerCrouch _playerCrouch;

        // State Variables
        private PlayerBaseState _currentState;
        private bool _isCrouching;
        private bool _isCrouchPressedThisFrame; // Crouch için anlık basım
        private bool _isJumpPressed;
        private bool _isMovementPressed;
        private bool _isRunPressed;
        private PlayerStateFactory _states;
        private float _verticalVelocity;

        // Getters & Setters
        public PlayerStateMachine(PlayerController playerController,
            CharacterController characterController,
            PlayerInput playerInput,
            Camera playerCamera,
            PlayerData playerData,
            PlayerCrouch playerCrouch) : base(playerController)
        {
            _characterController = characterController;
            _playerInput = playerInput;
            _playerCamera = playerCamera;
            _playerData = playerData;
            _playerCrouch = playerCrouch;
        }

        public CharacterController CharacterController => _characterController;
        public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
        public Vector2 CurrentMovementInput => _currentMovementInput;
        public bool IsMovementPressed => _isMovementPressed;
        public bool IsRunPressed => _isRunPressed;
        public bool IsJumpPressed => _isJumpPressed;
        public bool IsCrouchPressedThisFrame => _isCrouchPressedThisFrame;
        public bool IsCrouching { get => _isCrouching; set => _isCrouching = value; }
        public float CurrentMovementY { get => _verticalVelocity; set => _verticalVelocity = value; }
        public PlayerCrouch PlayerCrouch => _playerCrouch;
        
        // X ve Z hareketini state'ler ayarlar
        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }

        public override void Awake()
        {
            _states = new PlayerStateFactory(this);
            _currentState = _states.Idle();
            _currentState.EnterState();
        }

        public override void Update()
        {
            HandleInput();
            _currentState.UpdateState();
            HandleMovement();
            HandleGravity();
        }

        public override void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        private void HandleInput()
        {
            _currentMovementInput = _playerInput.moveInput;
            _isMovementPressed = _playerInput.isMoveInput;

            _isRunPressed = _playerInput.isRunPressed;
            _isJumpPressed = _playerInput.isJumpPressed;
            _isCrouchPressedThisFrame = _playerInput.isCrouchPressed;
        }

        private void HandleMovement()
        {
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);

            // Eğer karakterin baktığı yöne gitmesini istiyorsanız:
            moveDirection = _playerCamera.transform.TransformDirection(moveDirection);
            
            // Yerçekimi ve zıplama
            moveDirection.y = _verticalVelocity;
            _characterController.Move(moveDirection * Time.deltaTime);
            
            // // Karakteri hareket yönüne döndürme (sadece yatay hareket varsa)
            // if (_isMovementPressed && (AppliedMovementX != 0 || AppliedMovementZ != 0))
            // {
            //     var horizontalMove = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            //     // Eğer transform.TransformDirection kullanıyorsanız, dönüşü input'a göre yapın:
            //     var inputDirection = new Vector3(_currentMovementInput.x, 0, _currentMovementInput.y).normalized;
            //     if (inputDirection != Vector3.zero)
            //     {
            //         // Karakterin doğrudan input yönüne bakması için:
            //         // Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            //         // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            //
            //         // Veya karakterin gittiği yöne bakması için (moveDirection'ı normalize etmeden önce)
            //         var lookDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
            //         if (lookDirection.sqrMagnitude > 0.01f) // Çok küçük hareketlerde dönmeyi engelle
            //         {
            //             var targetRotation = Quaternion.LookRotation(lookDirection.normalized);
            //             _playerCamera.transform.rotation = Quaternion.Slerp(PlayerController.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            //         }
            //     }
            // }
        }

        private void HandleGravity()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f; // Yere yapışması için küçük bir negatif kuvvet
            }
            else
            {
                _verticalVelocity -= Gravity * Time.deltaTime;
            }
        }
    }
}