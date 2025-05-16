using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;
using PrimeTween;

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
        
        private void Initialize()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            _initialCameraYLocalPosition = playerCamera != null ? 
                playerCamera.localPosition.y : 0f;
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

            var startCenterY = characterController.center.y;
            var endCenterY = playerData.isCrouching ? playerData.crouchHeight / 2f : 0f;

            // Animate height
            Tween.Custom(startHeight, endHeight, playerData.crouchTransitionTime,
                onValueChange: newHeight =>
                {
                    characterController.height = newHeight;
                }, Ease.OutQuad
            );

            // Animate center.y
            Tween.Custom(startCenterY, endCenterY, playerData.crouchTransitionTime,
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

            Tween.Custom(startCameraY, endCameraY, playerData.crouchTransitionTime,
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
            var origin = new Vector3(characterController.transform.position.x, 
                characterController.height, characterController.transform.position.z);
            var rayDistance = playerData.ceilingCheckDistance;
            
            return !Physics.Raycast(origin, Vector3.up, rayDistance,
                playerData.ceilingLayers);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            var origin = new Vector3(characterController.transform.position.x, 
                characterController.height, characterController.transform.position.z);
            var rayDistance = playerData.ceilingCheckDistance;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(origin, 0.05f);
            Gizmos.DrawWireSphere(origin + Vector3.up * rayDistance, 0.05f);
        }
#endif
    }
}