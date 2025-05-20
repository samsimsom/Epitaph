using System;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using PrimeTween;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerCrouch : PlayerBehaviour
    {
        public PlayerCrouch(PlayerController playerController, 
            PlayerMovementData playerMovementData, 
            CharacterController characterController, 
            Camera playerCamera) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            _characterController = characterController;
            _playerCamera = playerCamera;
            
            Initialize();
        }

        public static event Action<bool> OnCrouchStateChanged;
        public static event Action<float> OnChangeCrouchSpeed;
        public static event Action<float> OnChangeGroundedGravity;
        
        private PlayerMovementData _playerMovementData;
        
        private CharacterController _characterController;
        private Camera _playerCamera;
        
        private float _initialCameraYLocalPosition;
        
        public override void Start()
        {
            
        }

        public override void OnEnable()
        {
        }
        
        public override void OnDisable()
        {
        }
        
        public override void Update()
        {
            // Eğer crouch durumunda ve isFalling true olduysa, otomatik Stand'a dönüş
            if (_playerMovementData.isCrouching && _playerMovementData.isFalling)
            {
                Stand();
                // İsteğe bağlı: Hızlıca transition uygula veya animasyon tetikle
                SmoothCrouchTransition();
            }
        }
        
        private void Initialize()
        {
            _initialCameraYLocalPosition = _playerCamera != null ? 
                _playerCamera.transform.localPosition.y : 0f;
            
            _playerMovementData.standingHeight = _characterController.height;
        }
        
        public void ToggleCrouch()
        {
            if (_playerMovementData.isCrouching)
            {
                Stand();
            }
            else
            {
                if (_playerMovementData.isGrounded)
                {
                    Crouch();
                }
            }
            // Always call this to handle smooth transition for both crouch and stand
            SmoothCrouchTransition();
        }

        private void Crouch()
        {
            _playerMovementData.isCrouching = true;
            OnCrouchStateChanged?.Invoke(_playerMovementData.isCrouching);
            OnChangeGroundedGravity?.Invoke(_playerMovementData.crouchGroundedGravity);
            OnChangeCrouchSpeed?.Invoke(_playerMovementData.crouchSpeed);
        }

        private void Stand()
        {
            if (!CanStandUp()) return;
            _playerMovementData.isCrouching = false;
            OnCrouchStateChanged?.Invoke(_playerMovementData.isCrouching);
            OnChangeGroundedGravity?.Invoke(_playerMovementData.groundedGravity);
            OnChangeCrouchSpeed?.Invoke(_playerMovementData.walkSpeed);
        }

        private void SmoothCrouchTransition()
        {
            var startHeight = _characterController.height;
            var endHeight = _playerMovementData.isCrouching ? _playerMovementData.crouchHeight : _playerMovementData.standingHeight;
            var duration = _playerMovementData.isCrouching ? _playerMovementData.crouchTransitionTime : _playerMovementData.crouchTransitionTime / 2f;

            var startCenterY = _characterController.center.y;
            var endCenterY = _playerMovementData.isCrouching ? _playerMovementData.crouchHeight / 2f : 0f;

            // Animate height
            Tween.Custom(startHeight, endHeight, duration,
                onValueChange: newHeight =>
                {
                    _characterController.height = newHeight;
                }, Ease.OutQuad
            );

            // Animate center.y
            Tween.Custom(startCenterY, endCenterY, duration,
                onValueChange: newCenterY =>
                {
                    var center = _characterController.center;
                    center.y = newCenterY;
                    _characterController.center = center;
                }, Ease.OutQuad
            );

            // Animate camera position for smoother effect
            if (_playerCamera == null) return;
            
            var startCameraY = _playerCamera.transform.localPosition.y;
            var endCameraY = _initialCameraYLocalPosition + 
                             (_playerMovementData.isCrouching ? _playerMovementData.crouchCameraYOffset : 
                                 _playerMovementData.standingCameraYOffset);

            Tween.Custom(startCameraY, endCameraY, duration,
                onValueChange: newCameraY =>
                {
                    var camPos = _playerCamera.transform.localPosition;
                    camPos.y = newCameraY;
                    _playerCamera.transform.localPosition = camPos;
                }, Ease.OutQuad
            );
            
        }

        private bool CanStandUp()
        {
            if (_characterController == null) return false;
            
            ComputeCeilingRayOrigin(out var radius, out var rayDistance, 
                out var originTip, out var originRoot);
            
            var raycast = !Physics.Raycast(originRoot, Vector3.up, rayDistance, _playerMovementData.ceilingLayers);
            var raySphere = !Physics.CheckSphere(originTip, radius, _playerMovementData.ceilingLayers);
            
            return raycast && raySphere;
        }

        private void ComputeCeilingRayOrigin(out float radius, 
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = _characterController.radius;
            rayDistance = _playerMovementData.ceilingCheckDistance;
            originTip = _characterController.transform.position
                        + _characterController.center
                        + Vector3.up * (_characterController.height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = _characterController.transform.position
                         + _characterController.center
                         + Vector3.up * (_characterController.height / 2f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_characterController == null) return;
            
            ComputeCeilingRayOrigin(out var radius, out var rayDistance, 
                out var originTip, out var originRoot);
            
            var color = CanStandUp() ? Color.green : Color.red;
            Gizmos.color = color;
            Gizmos.DrawLine(originRoot, originRoot + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(originTip, radius);
        }
#endif
    }
}