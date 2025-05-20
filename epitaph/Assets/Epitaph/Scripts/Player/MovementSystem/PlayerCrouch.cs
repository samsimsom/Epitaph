using System;
using Epitaph.Scripts.Player.ScriptableObjects;
using PrimeTween;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerCrouch : PlayerBehaviour
    {
        public PlayerCrouch(PlayerController playerController, 
            PlayerData playerData, 
            CharacterController characterController, 
            Camera playerCamera) : base(playerController)
        {
            _playerData = playerData;
            _characterController = characterController;
            _playerCamera = playerCamera;
            
            Initialize();
        }

        public static event Action<bool> OnCrouchStateChanged;
        public static event Action<float> OnChangeCrouchSpeed;
        public static event Action<float> OnChangeGroundedGravity;
        
        private PlayerData _playerData;
        
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
            if (_playerData.isCrouching && _playerData.isFalling)
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
            
            _playerData.standingHeight = _characterController.height;
        }
        
        public void ToggleCrouch()
        {
            if (_playerData.isCrouching)
            {
                Stand();
            }
            else
            {
                if (_playerData.isGrounded)
                {
                    Crouch();
                }
            }
            // Always call this to handle smooth transition for both crouch and stand
            SmoothCrouchTransition();
        }

        private void Crouch()
        {
            _playerData.isCrouching = true;
            OnCrouchStateChanged?.Invoke(_playerData.isCrouching);
            OnChangeGroundedGravity?.Invoke(_playerData.crouchGroundedGravity);
            OnChangeCrouchSpeed?.Invoke(_playerData.crouchSpeed);
        }

        private void Stand()
        {
            if (!CanStandUp()) return;
            _playerData.isCrouching = false;
            OnCrouchStateChanged?.Invoke(_playerData.isCrouching);
            OnChangeGroundedGravity?.Invoke(_playerData.groundedGravity);
            OnChangeCrouchSpeed?.Invoke(_playerData.walkSpeed);
        }

        private void SmoothCrouchTransition()
        {
            var startHeight = _characterController.height;
            var endHeight = _playerData.isCrouching ? _playerData.crouchHeight : _playerData.standingHeight;
            var duration = _playerData.isCrouching ? _playerData.crouchTransitionTime : _playerData.crouchTransitionTime / 2f;

            var startCenterY = _characterController.center.y;
            var endCenterY = _playerData.isCrouching ? _playerData.crouchHeight / 2f : 0f;

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
                             (_playerData.isCrouching ? _playerData.crouchCameraYOffset : 
                                 _playerData.standingCameraYOffset);

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
            
            var raycast = !Physics.Raycast(originRoot, Vector3.up, rayDistance, _playerData.ceilingLayers);
            var raySphere = !Physics.CheckSphere(originTip, radius, _playerData.ceilingLayers);
            
            return raycast && raySphere;
        }

        private void ComputeCeilingRayOrigin(out float radius, 
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = _characterController.radius;
            rayDistance = _playerData.ceilingCheckDistance;
            originTip = _characterController.transform.position
                        + _characterController.center
                        + Vector3.up * (_characterController.height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = _characterController.transform.position
                         + _characterController.center
                         + Vector3.up * (_characterController.height / 2f);
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
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