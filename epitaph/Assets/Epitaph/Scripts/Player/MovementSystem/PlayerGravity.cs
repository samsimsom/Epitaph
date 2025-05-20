using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerGravity : PlayerBehaviour
    { 
        public PlayerGravity(PlayerController playerController, 
            PlayerMovementData playerMovementData, 
            CharacterController characterController) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            _characterController = characterController;
            
            Initialize();
        }
        
        private PlayerMovementData _playerMovementData;
        private CharacterController _characterController;
        
        private float _steepSlopeTime = 0f;
        
        private float _ungroundedTime;
        private bool _isFalling;
        
        private float _groundedGravity;
        private bool _isGrounded;

        private static float Gravity => Physics.gravity.y;

        public override void Start()
        {
            
        }

        public override void OnEnable()
        {
            
        }

        public override void OnDisable()
        {
            
        }

        private void Initialize()
        {
            _groundedGravity = _playerMovementData.groundedGravity;
            _ungroundedTime = 0f;
            _playerMovementData.isFalling = false;
        }

        public override void Update()
        {
            UpdateGroundedStatus();
            UpdateFallingStatus();
            ApplyGravity();
        }

        private void UpdateGroundedStatus()
        {
            _isGrounded = PerformGroundCheck();
            _playerMovementData.isGrounded = _isGrounded;
        }
        
        private void UpdateFallingStatus()
        {
            if (!_isGrounded)
            {
                _ungroundedTime += Time.deltaTime;
                if (_ungroundedTime > _playerMovementData.fallThreshold)
                {
                    _playerMovementData.isFalling = true;
                }
            }
            else
            {
                _ungroundedTime = 0f;
                _playerMovementData.isFalling = false;
            }
        }

        private bool PerformGroundCheck()
        {
            if (_characterController == null) return false;
            
            ComputeGroundCheckSphere(out var radius, out var origin);
            return Physics.CheckSphere(origin, radius, _playerMovementData.groundLayers);
        }

        private void ComputeGroundCheckSphere(out float radius, out Vector3 origin)
        {
            radius = _characterController.radius;
            origin = _characterController.transform.position 
                     + _characterController.center 
                     + Vector3.down * (_characterController.height / 2f);
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _playerMovementData.verticalVelocity < 0)
            {
                _playerMovementData.verticalVelocity = _groundedGravity;
            }
            else
            {
                _playerMovementData.verticalVelocity += Gravity * _playerMovementData.gravityMultiplier * Time.deltaTime;

                // Terminal hız sınırı kontrolü
                if (_playerMovementData.verticalVelocity < _playerMovementData.maxFallSpeed)
                    _playerMovementData.verticalVelocity = _playerMovementData.maxFallSpeed;
            }

            var movement = Vector3.up * (_playerMovementData.verticalVelocity * Time.deltaTime);
            HandleSlope(ref movement);
    
            _characterController.Move(movement);
            _playerMovementData.currentVelocity.y = _characterController.velocity.y;
        }


        private void HandleSlope(ref Vector3 moveDirection)
        {
            if (!_isGrounded) {
                _steepSlopeTime = 0f;
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(PlayerController.transform.position, Vector3.down, out hit,
                       (_characterController.height / 2f) + _playerMovementData.slopeForceRayLength,
                       _playerMovementData.groundLayers))
            {
                var slopeNormal = hit.normal;
                var slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
                if (slopeAngle > _characterController.slopeLimit)
                {
                    _steepSlopeTime += Time.deltaTime;
                    if (_steepSlopeTime >= _playerMovementData.slopeClimbThreshold)
                    {
                        // Kaymaya başla
                        var slopeDirection = Vector3.Cross(
                            Vector3.Cross(Vector3.up, slopeNormal), slopeNormal);
                        var slideAmount = Mathf.Clamp(
                            _playerMovementData.slideVelocity + (slopeAngle / 90f) * _playerMovementData.slopeForce,
                            0, _playerMovementData.maxSlideSpeed
                        );
                        moveDirection += slopeDirection.normalized * (slideAmount * Time.deltaTime);
                        _playerMovementData.verticalVelocity = _playerMovementData.groundedGravity * 2;
                    }
                    // (opsiyonel) threshold süresi dolmadan burada oyuncu ilerleyebilir,
                    // yani ek bir şey yapmanız gerekmez
                }
                else
                {
                    _steepSlopeTime = 0f; // çok dik değil, sayaç sıfırlanır
                }
            }
            else
            {
                _steepSlopeTime = 0f;
            }
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (_characterController == null) return;
            
            ComputeGroundCheckSphere(out var radius, out var origin);
            
            var color = _isGrounded ? Color.green : Color.red;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(origin, radius);
        }
#endif
    }
}