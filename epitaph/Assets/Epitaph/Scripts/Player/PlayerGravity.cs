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

        private float _verticalVelocity;
        private bool _isGrounded;
        
        private static float Gravity => Physics.gravity.y;

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
            var radius = characterController.radius;
            
            return Physics.CheckSphere(origin, radius, groundLayers);
        }

        private void ApplyGravity()
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

        #region Public Methods
        public float GetVerticalVelocity() => _verticalVelocity;

        public void SetVerticalVelocity(float velocity) => _verticalVelocity = velocity;

        public bool IsGrounded() => _isGrounded;
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;

            Gizmos.color = Color.red;
            
            var origin = playerBody.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var radius = characterController.radius;
            
            Gizmos.DrawWireSphere(origin, radius);
        }
#endif
        
    }
}