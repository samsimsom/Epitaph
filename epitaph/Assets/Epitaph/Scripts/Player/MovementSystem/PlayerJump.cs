using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerJump : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        
        private bool _canJump = true;
        private float _jumpCooldownTimer;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private bool _wasGroundedLastFrame;
        
        private void Awake()
        {
            InitializeComponents();
        }

        private void OnEnable()
        {
            PlayerInput.OnJumpPerformed += ProcessJump;
        }

        private void OnDisable()
        {
            PlayerInput.OnJumpPerformed -= ProcessJump;
        }
        
        private void Update()
        {
            HandleJumpCooldown();
            HandleCoyoteTime();
            HandleJumpBuffer();
        }

        private void InitializeComponents()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }
        
        private void HandleJumpCooldown()
        {
            if (_canJump) return;
            
            _jumpCooldownTimer -= Time.deltaTime;
            if (_jumpCooldownTimer <= 0)
            {
                _canJump = true;
            }
        }
        
        private void HandleCoyoteTime()
        {
            if (!playerMovementData.useCoyoteTime) return;
            
            var isGrounded = playerMovementData.isGrounded;
            
            // Yerdeyken coyote sayacını max değerine ayarla
            if (isGrounded)
            {
                _coyoteTimeCounter = playerMovementData.coyoteTime;
            }
            else
            {
                // Yerden düştüyse sayacı başlat
                if (_wasGroundedLastFrame)
                {
                    _wasGroundedLastFrame = false;
                }
                
                // Yerden düştükten sonra sayacı azalt
                _coyoteTimeCounter -= Time.deltaTime;
            }
            
            _wasGroundedLastFrame = isGrounded;
        }
        
        private void HandleJumpBuffer()
        {
            if (_jumpBufferCounter > 0)
            {
                _jumpBufferCounter -= Time.deltaTime;
                
                // Eğer buffer süresi içinde yere değerse zıpla
                if (CanJump())
                {
                    ExecuteJump();
                    _jumpBufferCounter = 0;
                }
            }
        }
        
        public bool CanJump()
        {
            ComputeCeilingRayOrigin(out var radius, out var rayDistance, 
                out var originTip, out var originRoot);
            var cannotHitCeiling = !Physics.Raycast(originRoot, Vector3.up, rayDistance,
                playerMovementData.ceilingLayers);
            var isInCoyoteTime = playerMovementData.useCoyoteTime && _coyoteTimeCounter > 0;
            
            return !playerMovementData.isCrouching && _canJump && (playerMovementData.isGrounded || isInCoyoteTime) && cannotHitCeiling;
        }
        
        private void ComputeCeilingRayOrigin(out float radius, 
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = characterController.radius;
            rayDistance = playerMovementData.ceilingCheckDistance;
            originTip = characterController.transform.position
                        + characterController.center
                        + Vector3.up * (characterController.height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = characterController.transform.position
                         + characterController.center
                         + Vector3.up * (characterController.height / 2f);
        }
        
        public void ProcessJump()
        {
            if (CanJump())
            {
                ExecuteJump();
            }
            else
            {
                // Zıplayamasa bile jump buffer'ı başlat
                _jumpBufferCounter = playerMovementData.jumpBufferTime;
            }
        }
        
        private void ExecuteJump()
        {
            var jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * 
                                          playerMovementData.jumpHeight);
            playerMovementData.verticalVelocity = jumpVelocity;
    
            _canJump = false;
            _jumpCooldownTimer = playerMovementData.jumpCooldown;
            _coyoteTimeCounter = 0; // Zıpladıktan sonra coyote time'ı sıfırla
        }

        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            ComputeCeilingRayOrigin(out var radius, out var rayDistance, 
                out var originTip, out var originRoot);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(originRoot, originRoot + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(originRoot, 0.05f);
            Gizmos.DrawWireSphere(originRoot + Vector3.up * rayDistance, 0.05f);
        }
#endif

    }
}