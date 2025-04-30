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
        [SerializeField] private float groundCheckDistance = 0.2f;

        private float _verticalVelocity;
        private bool _isGrounded;
        private float Gravity => Physics.gravity.y;

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
            _isGrounded = PerformNativeGroundCheck() || PerformCustomGroundCheck();
#if UNITY_EDITOR
#if false
            Debug.Log($"Native IsGrounded: {PerformNativeGroundCheck()}, " +
                      $"Custom IsGrounded: {PerformCustomGroundCheck()}, Final: {_isGrounded}");
#endif
#endif
        }

        private bool PerformNativeGroundCheck()
        {
            return characterController.isGrounded;
        }

        private bool PerformCustomGroundCheck()
        {
            if (playerBody == null || characterController == null)
                return false;
            
            var rayDistance = characterController.height / 2 + groundCheckDistance;
            return Physics.Raycast(playerBody.position, Vector3.down, rayDistance,
                groundLayers);
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

        private void OnDrawGizmosSelected()
        {
            if (characterController == null || playerBody == null) return;

            Gizmos.color = Color.blue;
            var lineLen = characterController.height / 2 + groundCheckDistance;
            Gizmos.DrawLine(playerBody.position, playerBody.position + Vector3.down * lineLen);
            
        }
    }
}