using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // State Variables
        private BaseState _currentState;
        private StateFactory _states;
        
        // Movement Variables
        public float WalkSpeed = 2.5f;
        public float RunSpeed = 3.4f;
        public float CrouchSpeed = 1.5f;

        // Jump Variables
        public float JumpForce = 6.0f;
        public float Gravity = 20.0f;
        private float _verticalVelocity;

        // Crouch Variables
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);
        public float NormalCameraHeight = 1.5f;
        public float CrouchCameraHeight = 0.6f;
        private bool _isCrouching;

        // Input Variables
        private Vector2 _currentMovementInput;

        // Getters & Setters
        public BaseState CurrentState 
        { 
            get => _currentState;
            set => _currentState = value;
        }
        public bool IsCrouching
        {
            get => _isCrouching;
            set => _isCrouching = value;
        }
        public float CurrentMovementY
        {
            get => _verticalVelocity;
            set => _verticalVelocity = value;
        }
        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }

        // ---------------------------------------------------------------------------- //
        
        public MovementBehaviour(PlayerController playerController)
            : base(playerController) { }
        
        // ---------------------------------------------------------------------------- //
        
        public override void Awake()
        {
            _states = new StateFactory(this);
            _currentState = _states.Idle();
            _currentState.EnterState();
        }
        
        public override void Update()
        {
            _currentState.UpdateState();
            HandleMovement();
            HandleGravity();
        }
        
        public override void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        private void HandleMovement()
        {
            // Move
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            moveDirection = PlayerController.PlayerCamera.transform.TransformDirection(moveDirection);

            // Yerçekimi ve zıplama
            moveDirection.y = _verticalVelocity;
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);
            
        }

        private void HandleGravity()
        {
            if (PlayerController.CharacterController.isGrounded && _verticalVelocity < 0)
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
                UpdateCameraHeight(true);
                PlayerController.CharacterController.height = CrouchHeight;
                PlayerController.CharacterController.center = CrouchControllerCenter;
                IsCrouching = true;
            }
            else
            {
                // Ayağa kalkmadan önce üstte engel var mı kontrolü yapılmalı!
                if (CanStandUp())
                {
                    UpdateCameraHeight(false);
                    PlayerController.CharacterController.height = NormalHeight;
                    PlayerController.CharacterController.center = NormalControllerCenter;
                    _isCrouching = false;
                }
                else
                {
                    _isCrouching = true; // State'in tekrar crouch'a dönmesini sağlayabilir
                    UpdateCameraHeight(true);
                }
            }
        }
        
        public void UpdateCameraHeight(bool crouch)
        {
            var cameraTransform = PlayerController.CameraTransform;
            var newY = crouch ? CrouchCameraHeight : NormalCameraHeight;

            var targetPosition = new Vector3(
                cameraTransform.localPosition.x,
                newY,
                cameraTransform.localPosition.z);

            cameraTransform.localPosition = targetPosition;

            // Eğer yumuşak geçiş istiyorsan:
            // cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, 
            //     targetPosition, Time.deltaTime * 10f);
        }

        public bool CanStandUp()
        {
            // Karakterin başının üstünü kontrol et
            // Biraz yukarıdan başlayıp normal boy kadar bir kapsül/sphere cast yap
            var radius = PlayerController.CharacterController.radius;
            
            // Ayağa kalkarken ne kadar yukarı çıkacak
            var castDistance = NormalHeight - CrouchHeight;
            
            // Kapsülün tepesi
            var castStartPoint = PlayerController.CharacterController.transform.position + 
                                 CrouchControllerCenter + Vector3.up * (CrouchHeight / 2 - radius);

            // Raycast veya SphereCast daha iyi olabilir
            Debug.DrawRay(castStartPoint, Vector3.up * castDistance, Color.red, 2f);
            if (Physics.SphereCast(castStartPoint, radius, Vector3.up, 
                    out var hit, castDistance, ~LayerMask.GetMask("Player")))
            {
                // Bir şeye çarptı, ayağa kalkamaz
                Debug.Log("Cannot stand up, hit: " + hit.collider.name);
                return false;
            }
            return true;
        }
        
    }
}