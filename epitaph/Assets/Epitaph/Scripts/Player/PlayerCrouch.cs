using System;
using Epitaph.Scripts.Player.PlayerSO;
using PrimeTween;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerCrouch : MonoBehaviour
    {
        public static event Action<bool> OnCrouchStateChanged;
        public static event Action<float> OnChangeCrouchSpeed;
        public static event Action<float> OnChangeGroundedGravity;
        
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerCamera;
        
        private float _initialCameraYLocalPosition;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            PlayerInput.OnCrouchActivated += OnCrouchActivated;
            PlayerInput.OnCrouchDeactivated += OnCrouchDeactivated;
        }
        
        private void OnDisable()
        {
            PlayerInput.OnCrouchActivated -= OnCrouchActivated;
            PlayerInput.OnCrouchDeactivated -= OnCrouchDeactivated;
        }
        
        private void Update()
        {
            // Eğer crouch durumunda ve isFalling true olduysa, otomatik Stand'a dönüş
            if (playerData.isCrouching && playerData.isFalling)
            {
                Stand();
                // İsteğe bağlı: Hızlıca transition uygula veya animasyon tetikle
                SmoothCrouchTransition();
            }
        }
        
        private void Initialize()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            _initialCameraYLocalPosition = playerCamera != null ? 
                playerCamera.localPosition.y : 0f;
            
            playerData.standingHeight = characterController.height;
        }
        
        private void OnCrouchActivated()
        {
            if (playerData.isCrouching)
            {
                Stand();
            }
            else
            {
                Crouch();
            }
            // Always call this to handle smooth transition for both crouch and stand
            SmoothCrouchTransition();
        }
        
        private void OnCrouchDeactivated() { }

        private void Crouch()
        {
            playerData.isCrouching = true;
            OnCrouchStateChanged?.Invoke(playerData.isCrouching);
            OnChangeGroundedGravity?.Invoke(playerData.crouchGroundedGravity);
            OnChangeCrouchSpeed?.Invoke(playerData.crouchSpeed);
        }

        private void Stand()
        {
            if (!CanStandUp()) return;
            playerData.isCrouching = false;
            OnCrouchStateChanged?.Invoke(playerData.isCrouching);
            OnChangeGroundedGravity?.Invoke(playerData.groundedGravity);
            OnChangeCrouchSpeed?.Invoke(playerData.walkSpeed);
        }

        private void SmoothCrouchTransition()
        {
            var startHeight = characterController.height;
            var endHeight = playerData.isCrouching ? playerData.crouchHeight : playerData.standingHeight;
            var duration = playerData.isCrouching ? playerData.crouchTransitionTime : playerData.crouchTransitionTime / 2f;

            var startCenterY = characterController.center.y;
            var endCenterY = playerData.isCrouching ? playerData.crouchHeight / 2f : 0f;

            // Animate height
            Tween.Custom(startHeight, endHeight, duration,
                onValueChange: newHeight =>
                {
                    characterController.height = newHeight;
                }, Ease.OutQuad
            );

            // Animate center.y
            Tween.Custom(startCenterY, endCenterY, duration,
                onValueChange: newCenterY =>
                {
                    var center = characterController.center;
                    center.y = newCenterY;
                    characterController.center = center;
                }, Ease.OutQuad
            );

            // Animate camera position for smoother effect
            if (playerCamera == null) return;
            
            var startCameraY = playerCamera.localPosition.y;
            var endCameraY = _initialCameraYLocalPosition + 
                             (playerData.isCrouching ? playerData.crouchCameraYOffset : 
                                 playerData.standingCameraYOffset);

            Tween.Custom(startCameraY, endCameraY, duration,
                onValueChange: newCameraY =>
                {
                    var camPos = playerCamera.localPosition;
                    camPos.y = newCameraY;
                    playerCamera.localPosition = camPos;
                }, Ease.OutQuad
            );
            
        }

        private bool CanStandUp()
        {
            if (characterController == null) return false;
            
            ComputeCeilingRayOrigin(out var radius, out var rayDistance, 
                out var originTip, out var originRoot);
            
            var raycast = !Physics.Raycast(originRoot, Vector3.up, rayDistance, playerData.ceilingLayers);
            var raySphere = !Physics.CheckSphere(originTip, radius, playerData.ceilingLayers);
            
            return raycast && raySphere;
        }

        private void ComputeCeilingRayOrigin(out float radius, 
            out float rayDistance, out Vector3 originTip, out Vector3 originRoot)
        {
            radius = characterController.radius;
            rayDistance = playerData.ceilingCheckDistance;
            originTip = characterController.transform.position
                        + characterController.center
                        + Vector3.up * (characterController.height / 2f)
                        + Vector3.up * rayDistance;
            originRoot = characterController.transform.position
                         + characterController.center
                         + Vector3.up * (characterController.height / 2f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
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