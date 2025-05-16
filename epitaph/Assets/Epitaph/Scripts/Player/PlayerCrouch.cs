using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;
using PrimeTween;

namespace Epitaph.Scripts.Player
{
    public class PlayerCrouch : MonoBehaviour
    {
        // TODO: Karakter yuksek biyerde crouch yapiyorsa, groundedGravity'i -100 kaliyor.'
        // Bunu cozmenin yolunu bul.
        
        public static event Action<float> OnChangeCrouchSpeed;
        public static event Action<float> OnChangeGroundedGravity;
        
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerCamera;
        
        [Header("State (ReadOnly)")]
        [SerializeField] private bool isCrouching;
        
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
            OnCrouchPerformed();
        }
        
        private void OnCrouchDeactivated() { }

        private void OnCrouchPerformed()
        {
            if (isCrouching)
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

        private void Crouch()
        {
            isCrouching = true;
            OnChangeGroundedGravity?.Invoke(playerData.crouchGroundedGravity);
            OnChangeCrouchSpeed?.Invoke(playerData.crouchSpeed);
        }

        private void Stand()
        {
            if (!CanStandUp()) return;
            isCrouching = false;
            OnChangeGroundedGravity?.Invoke(playerData.groundedGravity);
            OnChangeCrouchSpeed?.Invoke(playerData.walkSpeed);
        }

        private void SmoothCrouchTransition()
        {
            var startHeight = characterController.height;
            var endHeight = isCrouching ? playerData.crouchHeight : playerData.standingHeight;

            var startCenterY = characterController.center.y;
            var endCenterY = isCrouching ? playerData.crouchHeight / 2f : 0f;

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
            if (playerCamera != null)
            {
                var startCameraY = playerCamera.localPosition.y;
                var endCameraY = _initialCameraYLocalPosition + 
                                 (isCrouching ? playerData.crouchCameraYOffset : 
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
        }

        private bool CanStandUp()
        {
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = playerData.ceilingCheckDistance;
            
            return !Physics.Raycast(origin, Vector3.up, rayDistance, playerData.ceilingLayers);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = playerData.ceilingCheckDistance;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(origin, 0.05f);
            Gizmos.DrawWireSphere(origin + Vector3.up * rayDistance, 0.05f);
        }
#endif
    }
}