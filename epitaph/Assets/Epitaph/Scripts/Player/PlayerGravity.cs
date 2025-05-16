using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerGravity : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;

        [Header("Gravity Settings")]
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float groundedGravity = -5f;
        [SerializeField] private float maxFallSpeed = -10f;

        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask groundLayers;

        [Header("State (ReadOnly)")]
        [SerializeField] private float verticalVelocity;
        
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
            // Inspectordaki debug text updateti icin.
            verticalVelocity = _verticalVelocity;
            
            UpdateGroundedStatus();
            ApplyGravity();
        }

        private void UpdateGroundedStatus()
        {
            _isGrounded = PerformGroundCheck();
        }

        private bool PerformGroundCheck()
        {
            if (characterController == null)
                return false;
            
            var origin = characterController.transform.position 
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
        
                // Terminal hız sınırı kontrolü
                if (_verticalVelocity < maxFallSpeed)
                    _verticalVelocity = maxFallSpeed;
            }

            var verticalMovement = Vector3.up * (_verticalVelocity * Time.deltaTime);
            characterController.Move(verticalMovement);
        }

        #region Public Methods
        public float GetVerticalVelocity() => _verticalVelocity;

        public void SetVerticalVelocity(float velocity) => _verticalVelocity = velocity;

        public bool IsGrounded() => _isGrounded;

        public void SetGroundedGravity(float gravity)
        {
            groundedGravity = gravity;
        }
        #endregion

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;

            Gizmos.color = Color.red;
            
            var origin = characterController.transform.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var radius = characterController.radius;
            
            Gizmos.DrawWireSphere(origin, radius);
        }
        #endif
        
    }
}