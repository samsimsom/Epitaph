using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerGravity : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private CharacterController characterController;

        [Header("Gravity Settings")]
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float groundedGravity = -0.5f;

        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundCheckDistance = 0.5f;

        private float _verticalVelocity;
        private bool _isGrounded;
        
        private static float Gravity => Physics.gravity.y;

        private bool _wasGroundedLastFrame;
        
        public delegate void GroundedStateHandler(bool isGrounded);
        public event GroundedStateHandler OnGroundedStateChanged;

        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }

        private void Update()
        {
            UpdateGroundedStatus();
            ApplyGravity();
        }

        private void UpdateGroundedStatus()
        {
            _wasGroundedLastFrame = _isGrounded;
            _isGrounded = PerformNativeGroundCheck() || PerformCustomGroundCheck();
            
            if (_wasGroundedLastFrame != _isGrounded)
            {
                OnGroundedStateChanged?.Invoke(_isGrounded);
            }
        }

        private bool PerformNativeGroundCheck()
        {
            return characterController.isGrounded;
        }

        private bool PerformCustomGroundCheck()
        {
            if (playerBody == null || characterController == null)
                return false;
            
            var origin = playerBody.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var rayDistance = groundCheckDistance;
            return Physics.Raycast(origin, Vector3.down, rayDistance, groundLayers);
        }

        public void ApplyGravity()
        {
            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = groundedGravity;
            }
            else
            {
                _verticalVelocity += Gravity * gravityMultiplier * Time.deltaTime;
            }

            var verticalMovement = Vector3.up * (_verticalVelocity * Time.deltaTime);
            characterController.Move(verticalMovement);
        }

        public float GetVerticalVelocity() => _verticalVelocity;

        public void SetVerticalVelocity(float velocity) => _verticalVelocity = velocity;

        public bool IsGrounded() => _isGrounded;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null || playerBody == null) return;

            Gizmos.color = Color.blue;
            var origin = playerBody.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var rayDistance = groundCheckDistance;
            
            Gizmos.DrawLine(origin, origin + Vector3.down * rayDistance);
            Gizmos.DrawWireSphere(origin, 0.05f);
            Gizmos.DrawWireSphere(origin + Vector3.down * rayDistance, 0.05f);
        }
#endif
        
    }
}