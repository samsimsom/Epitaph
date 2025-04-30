using UnityEngine;

namespace Epitaph.Scripts.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerCrouch : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private Transform playerCamera;

        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchSpeed = 2.0f;
        [SerializeField] private float standingSpeed = 5.0f;
        [SerializeField] private float crouchCameraYOffset = -0.5f;
        [SerializeField] private float standingCameraYOffset = 0.0f;
        [SerializeField] private float crouchTransitionTime = 0.2f;

        [Header("State (ReadOnly)")]
        [SerializeField] private bool isCrouching = false;

        private float _crouchTransitionTimer;
        private float _initialCameraYLocalPosition;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();
            
            if (playerCamera == null && playerMovement.GetPlayerCamera() != null)
                playerCamera = playerMovement.GetPlayerCamera().transform;
            
            _initialCameraYLocalPosition = playerCamera != null ? playerCamera.localPosition.y : 0f;
        }

        private void Update()
        {
            SmoothCrouchTransition();
        }

        public void OnCrouchPerformed() // Trigger this from your input system (toggle)
        {
            if (isCrouching)
                Stand();
            else
                Crouch();
        }

        private void Crouch()
        {
            isCrouching = true;
            _crouchTransitionTimer = 0f;
            playerMovement.SetMoveSpeed(crouchSpeed);
        }

        private void Stand()
        {
            // Headroom kontrolü (engininiz isteğe göre kontrol ekleyebilirsiniz)
            if (!CanStandUp()) return;

            isCrouching = false;
            _crouchTransitionTimer = 0f;
            playerMovement.SetMoveSpeed(standingSpeed);
        }

        private void SmoothCrouchTransition()
        {
            var goalHeight = isCrouching ? crouchHeight : standingHeight;
            var startHeight = characterController.height;
            var goalCenterY = isCrouching ? crouchHeight / 2f : 0;
            var startCenterY = characterController.center.y;

            var goalCameraY = _initialCameraYLocalPosition + (isCrouching ? crouchCameraYOffset : standingCameraYOffset);
            var startCameraY = (playerCamera != null) ? playerCamera.localPosition.y : 0f;

            if (Mathf.Approximately(startHeight, goalHeight) &&
                Mathf.Approximately(startCenterY, goalCenterY) &&
                Mathf.Approximately(startCameraY, goalCameraY))
                return;

            _crouchTransitionTimer += Time.deltaTime / crouchTransitionTime;

            characterController.height = Mathf.Lerp(startHeight, goalHeight, _crouchTransitionTimer);
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
            var origin = transform.position + Vector3.up * (crouchHeight / 2f);
            var checkDistance = (standingHeight - crouchHeight) * 0.9f;
            // Raycast ile kafa çarpması engelleniyor.
            return !Physics.Raycast(origin, Vector3.up, checkDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }

        public bool IsCrouching() => isCrouching;

        // Inspector üzerinden anlık olarak değiştirmek gerekirse:
        public void SetCrouching(bool crouch)
        {
            if (crouch && !isCrouching) Crouch();
            if (!crouch && isCrouching) Stand();
        }
    }
}