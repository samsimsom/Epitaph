using UnityEngine;

namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerStateMachine : PlayerBehaviour
    {
        // Movement Variables
        public float WalkSpeed = 1.75f;
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 1.25f;
        // private float _rotationSpeed = 720f; // Karakterin dönüş hızı
        public float JumpForce = 8.0f;
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

        // State Variables
        private PlayerBaseState _currentState;
        private bool _isCrouching;
        private bool _isCrouchPressed; // Crouch için anlık basım
        private bool _isJumpPressed;
        private bool _isMovementPressed;
        private bool _isRunPressed;
        private PlayerStateFactory _states;
        private float _verticalVelocity;

        // Getters & Setters
        public PlayerStateMachine(PlayerController playerController,
            CharacterController characterController,
            PlayerInput playerInput,
            Camera playerCamera) : base(playerController)
        {
            _characterController = characterController;
            _playerInput = playerInput;
            _playerCamera = playerCamera;
        }

        public CharacterController CharacterController => _characterController;
        public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
        public bool IsMovementPressed => _isMovementPressed;
        public bool IsRunPressed => _isRunPressed;
        public bool IsJumpPressed => _isJumpPressed;
        public bool IsCrouchPressed => _isCrouchPressed;
        public bool IsCrouching { get => _isCrouching; set => _isCrouching = value; }
        public float CurrentMovementY { get => _verticalVelocity; set => _verticalVelocity = value; }
        public float AppliedMovementX { get; set; } // X ve Z hareketini state'ler ayarlar
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
            _isCrouchPressed = _playerInput.isCrouchPressed;
        }

        private void HandleMovement()
        {
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            // Kameraya göre hareket için (eğer kamera karakteri takip ediyorsa)
            // moveDirection = Camera.main.transform.TransformDirection(moveDirection);
            // moveDirection.y = 0; // Y ekseninde kamera dönüşünden kaynaklı hareketi engelle
            
            // Dünya eksenlerine göre hareket (basit bir örnek)
            // Eğer karakterin baktığı yöne gitmesini istiyorsanız:
            moveDirection = _playerCamera.transform.TransformDirection(moveDirection);
            
            moveDirection.y = _verticalVelocity; // Yerçekimi ve zıplama
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

        public void ChangeCharacterControllerDimensions(bool crouch)
        {
            if (crouch)
            {
                _characterController.height = CrouchHeight;
                _characterController.center = CrouchControllerCenter;
            }
            else
            {
                // Ayağa kalkmadan önce üstte engel var mı kontrolü yapılmalı!
                if (CanStandUp())
                {
                    _characterController.height = NormalHeight;
                    _characterController.center = NormalControllerCenter;
                    _isCrouching = false;
                }
                else
                {
                    // Engel varsa, crouch'ta kal
                    _isCrouching = true; // State'in tekrar crouch'a dönmesini sağlayabilir
                }
            }
        }

        public bool CanStandUp()
        {
            // Karakterin başının üstünü kontrol et
            // Biraz yukarıdan başlayıp normal boy kadar bir kapsül/sphere cast yap
            var radius = _characterController.radius;
            var castDistance =
                NormalHeight - CrouchHeight; // Ayağa kalkarken ne kadar yukarı çıkacak
            var castStartPoint = _characterController.transform.position + CrouchControllerCenter + Vector3.up * (CrouchHeight / 2 - radius); // Kapsülün tepesi

            // Raycast veya SphereCast daha iyi olabilir
            // Debug.DrawRay(castStartPoint, Vector3.up * castDistance, Color.red, 2f);
            if (Physics.SphereCast(castStartPoint, radius, Vector3.up, out var hit, castDistance, ~LayerMask.GetMask("Player"))) // Player katmanını hariç tut
            {
                // Bir şeye çarptı, ayağa kalkamaz
                // Debug.Log("Cannot stand up, hit: " + hit.collider.name);
                return false;
            }

            return true;
        }
    }
}