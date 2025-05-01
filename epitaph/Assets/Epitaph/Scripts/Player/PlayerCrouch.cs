using UnityEngine;
using UnityEngine.Serialization;

namespace Epitaph.Scripts.Player
{
    [RequireComponent(typeof(PlayerMove))]
    public class PlayerCrouch : MonoBehaviour
    {
        public delegate void CrouchStateHandler(bool isCrouching);
        public event CrouchStateHandler OnCrouchStateChanged;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        // [SerializeField] private Transform playerBody;
        [FormerlySerializedAs("playerMovement")] [SerializeField] private PlayerMove playerMove;
        [SerializeField] private Transform playerCamera;

        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchSpeed = 2.0f;
        [SerializeField] private float crouchCameraYOffset = -0.5f;
        [SerializeField] private float standingCameraYOffset = 0.0f;
        [SerializeField] private float crouchTransitionTime = 0.2f;

        [Header("Ceil Check Settings")]
        [SerializeField] private LayerMask ceilingLayers;
        [SerializeField] private float ceilingCheckDistance = 0.5f;
        
        [Header("State (ReadOnly)")]
        [SerializeField] private bool isCrouching;
        
        private float _crouchTransitionTimer;
        private float _initialCameraYLocalPosition;
        private float _standingSpeed;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (playerMove == null)
                playerMove = GetComponent<PlayerMove>();
            
            _initialCameraYLocalPosition = playerCamera != null ? 
                playerCamera.localPosition.y : 0f;
            _standingSpeed = playerMove.GetMoveSpeed();
        }

        private void Update()
        {
            SmoothCrouchTransition();
        }

        public void OnCrouchPerformed()
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
            playerMove.SetMoveSpeed(crouchSpeed);
            OnCrouchStateChanged?.Invoke(true);
        }

        private void Stand()
        {
            // Headroom kontrolü
            if (!CanStandUp()) return;

            isCrouching = false;
            _crouchTransitionTimer = 0f;
            playerMove.SetMoveSpeed(_standingSpeed);
            OnCrouchStateChanged?.Invoke(false);
        }

        private void SmoothCrouchTransition()
        {
            var goalHeight = isCrouching ? crouchHeight : standingHeight;
            var startHeight = characterController.height;
            var goalCenterY = isCrouching ? crouchHeight / 2f : 0;
            var startCenterY = characterController.center.y;

            var goalCameraY = _initialCameraYLocalPosition + (isCrouching ? 
                crouchCameraYOffset : standingCameraYOffset);
            var startCameraY = (playerCamera != null) ? playerCamera.localPosition.y : 0f;

            if (Mathf.Approximately(startHeight, goalHeight) &&
                Mathf.Approximately(startCenterY, goalCenterY) &&
                Mathf.Approximately(startCameraY, goalCameraY))
                return;

            _crouchTransitionTimer += Time.deltaTime / crouchTransitionTime;

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
            var rayDistance = ceilingCheckDistance;
            
            return !Physics.Raycast(origin, Vector3.up, rayDistance, ceilingLayers);
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
            return crouchSpeed;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = ceilingCheckDistance;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(origin, 0.05f);
            Gizmos.DrawWireSphere(origin + Vector3.up * rayDistance, 0.05f);
        }
#endif
    

    }
}