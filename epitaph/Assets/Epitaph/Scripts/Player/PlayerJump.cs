using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerJump : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerGravity playerGravity;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpCooldown = 0.1f;
        
        [Header("Ceil Check Settings")]
        [SerializeField] private LayerMask ceilingLayers;
        [SerializeField] private float ceilingCheckDistance = 0.5f;
        
        // [Header("State (ReadOnly)")]
        // [SerializeField] private bool isJumping;
        
        private bool _canJump = true;
        private float _jumpCooldownTimer;
        
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
            
            if (playerGravity == null)
            {
                playerGravity = GetComponent<PlayerGravity>();
            }
        }
        
        private void Update()
        {
            HandleJumpCooldown();
        }
        
        private void HandleJumpCooldown()
        {
            if (_canJump) return;
            
            _jumpCooldownTimer -= Time.deltaTime;
            if (_jumpCooldownTimer <= 0)
            {
                _canJump = true;
            }
        }
        
        public bool CanJump()
        {
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = ceilingCheckDistance;
            
            return _canJump && playerGravity.IsGrounded() && 
                   !Physics.Raycast(origin, Vector3.up, rayDistance, ceilingLayers);
        }
        
        public void ProcessJump()
        {
            if (!CanJump()) return;
            
            var jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            playerGravity.SetVerticalVelocity(jumpVelocity);
            
            _canJump = false;
            _jumpCooldownTimer = jumpCooldown;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;
            
            var origin = characterController.transform.position + Vector3.up;
            var rayDistance = ceilingCheckDistance;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + Vector3.up * rayDistance);
            Gizmos.DrawWireSphere(origin, 0.05f);
            Gizmos.DrawWireSphere(origin + Vector3.up * rayDistance, 0.05f);
        }
#endif

    }
}