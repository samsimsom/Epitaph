using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerGravity : PlayerBehaviour
    { 
        public PlayerGravity(PlayerController playerController, 
            PlayerData playerData, 
            CharacterController characterController) : base(playerController)
        {
            _playerData = playerData;
            _characterController = characterController;
            
            Initialize();
        }
        
        private PlayerData _playerData;
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
            _groundedGravity = _playerData.groundedGravity;
            _ungroundedTime = 0f;
            _playerData.isFalling = false;
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
            _playerData.isGrounded = _isGrounded;
        }
        
        private void UpdateFallingStatus()
        {
            if (!_isGrounded)
            {
                _ungroundedTime += Time.deltaTime;
                if (_ungroundedTime > _playerData.fallThreshold)
                {
                    _playerData.isFalling = true;
                }
            }
            else
            {
                _ungroundedTime = 0f;
                _playerData.isFalling = false;
            }
        }

        private bool PerformGroundCheck()
        {
            if (_characterController == null) return false;
            
            ComputeGroundCheckSphere(out var radius, out var origin);
            return Physics.CheckSphere(origin, radius, _playerData.groundLayers);
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
            if (_isGrounded && _playerData.verticalVelocity < 0)
            {
                _playerData.verticalVelocity = _groundedGravity;
            }
            else
            {
                _playerData.verticalVelocity += Gravity * _playerData.gravityMultiplier * Time.deltaTime;

                // Terminal hız sınırı kontrolü
                if (_playerData.verticalVelocity < _playerData.maxFallSpeed)
                    _playerData.verticalVelocity = _playerData.maxFallSpeed;
            }

            var movement = Vector3.up * (_playerData.verticalVelocity * Time.deltaTime);
            HandleSlope(ref movement);
    
            _characterController.Move(movement);
            _playerData.currentVelocity.y = _characterController.velocity.y;
        }


        private void HandleSlope(ref Vector3 moveDirection)
        {
            if (!_isGrounded) {
                _steepSlopeTime = 0f;
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(PlayerController.transform.position, Vector3.down, out hit,
                       (_characterController.height / 2f) + _playerData.slopeForceRayLength,
                       _playerData.groundLayers))
            {
                var slopeNormal = hit.normal;
                var slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
                if (slopeAngle > _characterController.slopeLimit)
                {
                    _steepSlopeTime += Time.deltaTime;
                    if (_steepSlopeTime >= _playerData.slopeClimbThreshold)
                    {
                        // Kaymaya başla
                        var slopeDirection = Vector3.Cross(
                            Vector3.Cross(Vector3.up, slopeNormal), slopeNormal);
                        var slideAmount = Mathf.Clamp(
                            _playerData.slideVelocity + (slopeAngle / 90f) * _playerData.slopeForce,
                            0, _playerData.maxSlideSpeed
                        );
                        moveDirection += slopeDirection.normalized * (slideAmount * Time.deltaTime);
                        _playerData.verticalVelocity = _playerData.groundedGravity * 2;
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