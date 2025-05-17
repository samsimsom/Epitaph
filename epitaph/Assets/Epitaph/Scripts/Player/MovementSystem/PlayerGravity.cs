using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerGravity : MonoBehaviour

    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        
        private float _steepSlopeTime = 0f;
        
        private float _ungroundedTime;
        private bool _isFalling;
        
        private float _groundedGravity;
        private bool _isGrounded;
        
        private static float Gravity => Physics.gravity.y;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            PlayerCrouch.OnChangeGroundedGravity += groundedGravity =>
            {
                _groundedGravity = groundedGravity;
            };
        }

        private void OnDisable()
        {
            PlayerCrouch.OnChangeGroundedGravity -= groundedGravity =>
            {
                groundedGravity = _groundedGravity;
            };
        }

        private void Initialize()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            _groundedGravity = playerMovementData.groundedGravity;
            _ungroundedTime = 0f;
            playerMovementData.isFalling = false;
        }

        private void Update()
        {
            UpdateGroundedStatus();
            UpdateFallingStatus();
            ApplyGravity();
        }

        private void UpdateGroundedStatus()
        {
            _isGrounded = PerformGroundCheck();
            playerMovementData.isGrounded = _isGrounded;
        }
        
        private void UpdateFallingStatus()
        {
            if (!_isGrounded)
            {
                _ungroundedTime += Time.deltaTime;
                if (_ungroundedTime > playerMovementData.fallThreshold)
                {
                    playerMovementData.isFalling = true;
                }
            }
            else
            {
                _ungroundedTime = 0f;
                playerMovementData.isFalling = false;
            }
        }

        private bool PerformGroundCheck()
        {
            if (characterController == null) return false;
            
            ComputeGroundCheckSphere(out var radius, out var origin);
            return Physics.CheckSphere(origin, radius, playerMovementData.groundLayers);
        }

        private void ComputeGroundCheckSphere(out float radius, out Vector3 origin)
        {
            radius = characterController.radius;
            origin = characterController.transform.position 
                     + characterController.center 
                     + Vector3.down * (characterController.height / 2f);
        }

        private void ApplyGravity()
        {
            if (_isGrounded && playerMovementData.verticalVelocity < 0)
            {
                playerMovementData.verticalVelocity = _groundedGravity;
            }
            else
            {
                playerMovementData.verticalVelocity += Gravity * playerMovementData.gravityMultiplier * Time.deltaTime;

                // Terminal hız sınırı kontrolü
                if (playerMovementData.verticalVelocity < playerMovementData.maxFallSpeed)
                    playerMovementData.verticalVelocity = playerMovementData.maxFallSpeed;
            }

            var movement = Vector3.up * (playerMovementData.verticalVelocity * Time.deltaTime);
            HandleSlope(ref movement);
    
            characterController.Move(movement);
            playerMovementData.currentVelocity.y = characterController.velocity.y;
        }


        private void HandleSlope(ref Vector3 moveDirection)
        {
            if (!_isGrounded) {
                _steepSlopeTime = 0f;
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit,
                       (characterController.height / 2f) + playerMovementData.slopeForceRayLength,
                       playerMovementData.groundLayers))
            {
                var slopeNormal = hit.normal;
                var slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
                if (slopeAngle > characterController.slopeLimit)
                {
                    _steepSlopeTime += Time.deltaTime;
                    if (_steepSlopeTime >= playerMovementData.slopeClimbThreshold)
                    {
                        // Kaymaya başla
                        var slopeDirection = Vector3.Cross(
                            Vector3.Cross(Vector3.up, slopeNormal), slopeNormal);
                        var slideAmount = Mathf.Clamp(
                            playerMovementData.slideVelocity + (slopeAngle / 90f) * playerMovementData.slopeForce,
                            0, playerMovementData.maxSlideSpeed
                        );
                        moveDirection += slopeDirection.normalized * (slideAmount * Time.deltaTime);
                        playerMovementData.verticalVelocity = playerMovementData.groundedGravity * 2;
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
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            ComputeGroundCheckSphere(out var radius, out var origin);
            
            var color = _isGrounded ? Color.green : Color.red;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(origin, radius);
        }
        #endif
    }
}