using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerCrouch : MonoBehaviour
    {
        public delegate void CrouchStateHandler(bool isCrouching);
        public event CrouchStateHandler OnCrouchStateChanged;
        
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("References")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerCamera;
        
        [Header("State (ReadOnly)")]
        [SerializeField] private bool isCrouching;
        
        private float _crouchTransitionTimer;
        private float _initialCameraYLocalPosition;
        private float _standingSpeed;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            
            _initialCameraYLocalPosition = playerCamera != null ? 
                playerCamera.localPosition.y : 0f;
        }

        private void OnEnable()
        {
            playerInput.OnCrouchActivated += OnCrouchActivated;
            playerInput.OnCrouchDeactivated += OnCrouchDeactivated;
        }
        
        private void OnDisable()
        {
            playerInput.OnCrouchActivated -= OnCrouchActivated;
            playerInput.OnCrouchDeactivated -= OnCrouchDeactivated;
        }

        private void Update()
        {
            SmoothCrouchTransition();
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

        }

        private void Crouch()
        {
            isCrouching = true;
            _crouchTransitionTimer = 0f;
            OnCrouchStateChanged?.Invoke(true);
        }

        private void Stand()
        {
            if (!CanStandUp()) return;

            isCrouching = false;
            _crouchTransitionTimer = 0f;
            OnCrouchStateChanged?.Invoke(false);
        }

        private void SmoothCrouchTransition()
        {
            var goalHeight = isCrouching ? playerData.crouchHeight : playerData.standingHeight;
            var startHeight = characterController.height;
            var goalCenterY = isCrouching ? playerData.crouchHeight / 2f : 0;
            var startCenterY = characterController.center.y;

            var goalCameraY = _initialCameraYLocalPosition + (isCrouching ? 
                playerData.crouchCameraYOffset : playerData.standingCameraYOffset);
            var startCameraY = (playerCamera != null) ? playerCamera.localPosition.y : 0f;

            if (Mathf.Approximately(startHeight, goalHeight) &&
                Mathf.Approximately(startCenterY, goalCenterY) &&
                Mathf.Approximately(startCameraY, goalCameraY))
                return;

            _crouchTransitionTimer += Time.deltaTime / playerData.crouchTransitionTime;

            characterController.height = Mathf.Lerp(startHeight, goalHeight, 
                _crouchTransitionTimer);
            characterController.center = new Vector3(characterController.center.x,
                Mathf.Lerp(startCenterY, goalCenterY, _crouchTransitionTimer),
                characterController.center.z);

            if (playerCamera == null) return;
            var camLocalPos = playerCamera.localPosition;
            camLocalPos.y = Mathf.Lerp(startCameraY, goalCameraY, _crouchTransitionTimer);
            playerCamera.localPosition = camLocalPos;
        }

        private bool CanStandUp()
        {
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = playerData.ceilingCheckDistance;
            
            return !Physics.Raycast(origin, Vector3.up, rayDistance, playerData.ceilingLayers);
        }

        public bool IsCrouching() => isCrouching;
        
        public void SetCrouching(bool crouch)
        {
            switch (crouch)
            {
                case true when !isCrouching:
                    Crouch();
                    break;
                case false when isCrouching:
                    Stand();
                    break;
            }
        }
        
        public float GetCrouchSpeed()
        {
            return playerData.crouchSpeed;
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