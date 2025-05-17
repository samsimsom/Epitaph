#if true
using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerJump : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpCooldown = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        
        [Header("Coyote Time Settings")]
        [SerializeField] private float coyoteTime = 0.2f;
        [SerializeField] private bool useCoyoteTime = true;
        
        [Header("Ceil Check Settings")]
        [SerializeField] private LayerMask ceilingLayers;
        [SerializeField] private float ceilingCheckDistance = 0.5f;
        
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
            if (!useCoyoteTime) return;
            
            var isGrounded = playerMovementData.isGrounded;
            
            // Yerdeyken coyote sayacını max değerine ayarla
            if (isGrounded)
            {
                _coyoteTimeCounter = coyoteTime;
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
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = ceilingCheckDistance;
            var cannotHitCeiling = !Physics.Raycast(origin, Vector3.up, rayDistance,
                ceilingLayers);
            var isInCoyoteTime = useCoyoteTime && _coyoteTimeCounter > 0;
            
            return _canJump && (playerMovementData.isGrounded || isInCoyoteTime) && cannotHitCeiling;
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
                _jumpBufferCounter = jumpBufferTime;
            }
        }
        
        private void ExecuteJump()
        {
            var jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            // playerGravity.SetVerticalVelocity(jumpVelocity);
            
            _canJump = false;
            _jumpCooldownTimer = jumpCooldown;
            _coyoteTimeCounter = 0; // Zıpladıktan sonra coyote time'ı sıfırla
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = ceilingCheckDistance;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(origin, 0.05f);
            Gizmos.DrawWireSphere(origin + Vector3.up * rayDistance, 0.05f);
        }
#endif

    }
}
#endif