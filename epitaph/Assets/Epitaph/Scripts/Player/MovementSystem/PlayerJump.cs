using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerJump : PlayerBehaviour
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        
        private bool _canJump;
        private float _jumpCooldownTimer;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private bool _wasGroundedLastFrame;

        public PlayerJump(PlayerController playerController,
            PlayerData playerData) : base(playerController)
        {
            _playerController = playerController;
            _playerData = playerData;
        }

        public override void Update()
        {
            HandleJumpCooldown();
            HandleCoyoteTime();
            HandleJumpBuffer();
        }
        
        public bool CanJump()
        {
            ComputeCeilingRayOrigin(out var radius, out var rayDistance, out var originTip, out var originRoot);
            var cannotHitCeiling = !Physics.Raycast(originRoot, Vector3.up, rayDistance, _playerData.ceilingLayers);
            var isInCoyoteTime = _playerData.useCoyoteTime && _coyoteTimeCounter > 0;
            
            return !_playerData.isCrouching && _canJump && (_playerData.isGrounded || isInCoyoteTime) && cannotHitCeiling;
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
                _jumpBufferCounter = _playerData.jumpBufferTime;
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
            if (!_playerData.useCoyoteTime) return;
            
            var isGrounded = _playerData.isGrounded;
            
            // Yerdeyken coyote sayacını max değerine ayarla
            if (isGrounded)
            {
                _coyoteTimeCounter = _playerData.coyoteTime;
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
        
        private void ComputeCeilingRayOrigin(out float radius, 
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = _playerController.GetCharacterController().radius;
            rayDistance = _playerData.ceilingCheckDistance;
            originTip = _playerController.GetCharacterController().transform.position
                        + _playerController.GetCharacterController().center
                        + Vector3.up * (_playerController.GetCharacterController().height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = _playerController.GetCharacterController().transform.position
                         + _playerController.GetCharacterController().center
                         + Vector3.up * (_playerController.GetCharacterController().height / 2f);
        }
        
        private void ExecuteJump()
        {
            var jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * _playerData.jumpHeight);
            _playerData.verticalVelocity = jumpVelocity;
    
            _canJump = false;
            _jumpCooldownTimer = _playerData.jumpCooldown;
            _coyoteTimeCounter = 0; // Zıpladıktan sonra coyote time'ı sıfırla
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
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