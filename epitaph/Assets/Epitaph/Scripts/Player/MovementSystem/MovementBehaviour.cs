using System.Collections;
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
        public float CrouchCameraHeight = 0.7f;
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

        // Yeni eklenen alanlar
        private Coroutine _crouchTransitionCoroutine;
        public float CrouchTransitionDuration = 0.2f; // Geçiş süresi (saniye)
        
        // Kamera hareketi de yumuşak olsun istiyorsan:
        private Coroutine _cameraTransitionCoroutine;

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

        // ---------------------------------------------------------------------------- //

        #region Movement

        private void HandleMovement()
        {
            // Move
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            moveDirection = PlayerController.PlayerCamera.transform.TransformDirection(moveDirection);

            // Yerçekimi ve zıplama
            moveDirection.y = _verticalVelocity;
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);
            
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //

        #region Gravity

        private void HandleGravity()
        {
            if (PlayerController.CharacterController.isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -1f;
            }
            else
            {
                _verticalVelocity -= Gravity * Time.deltaTime;
            }
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //

        #region Crouching

        public void ChangeCharacterControllerDimensions(bool crouch)
        {
            if (_crouchTransitionCoroutine != null)
                PlayerController.StopCoroutine(_crouchTransitionCoroutine);

            if (crouch)
            {
                UpdateCameraHeightSmooth(true);
                _crouchTransitionCoroutine = PlayerController.StartCoroutine(SmoothCrouchTransition(
                    PlayerController.CharacterController.height, CrouchHeight,
                    PlayerController.CharacterController.center, CrouchControllerCenter,
                    CrouchTransitionDuration, true));
            }
            else
            {
                // Ayağa kalkmadan önce üstte engel var mı kontrolü yapılmalı!
                if (CanStandUp())
                {
                    UpdateCameraHeightSmooth(false);
                    _crouchTransitionCoroutine = PlayerController.StartCoroutine(SmoothCrouchTransition(
                        PlayerController.CharacterController.height, NormalHeight,
                        PlayerController.CharacterController.center, NormalControllerCenter,
                        CrouchTransitionDuration, false));
                }
                else
                {
                    _isCrouching = true;
                    UpdateCameraHeightSmooth(true);
                }
            }
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
        
        public void UpdateCameraHeightSmooth(bool crouch)
        {
            if (_cameraTransitionCoroutine != null)
                PlayerController.StopCoroutine(_cameraTransitionCoroutine);

            var targetY = crouch ? CrouchCameraHeight : NormalCameraHeight;
            _cameraTransitionCoroutine = PlayerController.StartCoroutine(
                SmoothCameraTransition(targetY, CrouchTransitionDuration));
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region Crouch Transition Coroutines
        
        private IEnumerator SmoothCrouchTransition(float fromHeight, float toHeight,
            Vector3 fromCenter, Vector3 toCenter, float duration, bool crouching)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                PlayerController.CharacterController.height = Mathf.Lerp(fromHeight, toHeight, t);
                PlayerController.CharacterController.center = Vector3.Lerp(fromCenter, toCenter, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            PlayerController.CharacterController.height = toHeight;
            PlayerController.CharacterController.center = toCenter;
            IsCrouching = crouching;
        }
        
        private IEnumerator SmoothCameraTransition(float targetY, float duration)
        {
            var cameraTransform = PlayerController.CameraTransform;
            var startPos = cameraTransform.localPosition;
            var endPos = new Vector3(startPos.x, targetY, startPos.z);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                cameraTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cameraTransform.localPosition = endPos;
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}