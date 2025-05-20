using System;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerJump : MonoBehaviour
    {
        private PlayerController _playerController;
        
        private bool _canJump = true;
        private float _jumpCooldownTimer;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private bool _wasGroundedLastFrame;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            HandleJumpCooldown();
            HandleCoyoteTime();
            HandleJumpBuffer();
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
            if (!_playerController.GetMovementData().useCoyoteTime) return;
            
            var isGrounded = _playerController.GetMovementData().isGrounded;
            
            // Yerdeyken coyote sayacını max değerine ayarla
            if (isGrounded)
            {
                _coyoteTimeCounter = _playerController.GetMovementData().coyoteTime;
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
                _playerController.GetMovementData().ceilingLayers);
            var isInCoyoteTime = _playerController.GetMovementData().useCoyoteTime && _coyoteTimeCounter > 0;
            
            return !_playerController.GetMovementData().isCrouching && _canJump && (_playerController.GetMovementData().isGrounded || isInCoyoteTime) && cannotHitCeiling;
        }
        
        private void ComputeCeilingRayOrigin(out float radius, 
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = _playerController.GetCharacterController().radius;
            rayDistance = _playerController.GetMovementData().ceilingCheckDistance;
            originTip = _playerController.GetCharacterController().transform.position
                        + _playerController.GetCharacterController().center
                        + Vector3.up * (_playerController.GetCharacterController().height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = _playerController.GetCharacterController().transform.position
                         + _playerController.GetCharacterController().center
                         + Vector3.up * (_playerController.GetCharacterController().height / 2f);
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
                _jumpBufferCounter = _playerController.GetMovementData().jumpBufferTime;
            }
        }
        
        private void ExecuteJump()
        {
            var jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * 
                                          _playerController.GetMovementData().jumpHeight);
            _playerController.GetMovementData().verticalVelocity = jumpVelocity;
    
            _canJump = false;
            _jumpCooldownTimer = _playerController.GetMovementData().jumpCooldown;
            _coyoteTimeCounter = 0; // Zıpladıktan sonra coyote time'ı sıfırla
        }

        
#if false
        private void OnDrawGizmos()
        {
            if (_playerController.GetCharacterController() == null) return;
            
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