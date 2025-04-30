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
            if (!_canJump)
            {
                _jumpCooldownTimer -= Time.deltaTime;
                if (_jumpCooldownTimer <= 0)
                {
                    _canJump = true;
                }
            }
        }
        
        public bool CanJump()
        {
            return _canJump && playerGravity.IsGrounded();
        }
        
        public void ProcessJump()
        {
            if (!CanJump()) return;
            
            // v = sqrt(2 * g * h) - zıplama için gereken hız formülü
            var jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            
            // Dikey hızı ayarla
            playerGravity.SetVerticalVelocity(jumpVelocity);
            
            // Zıplama sonrası soğuma süresini başlat
            _canJump = false;
            _jumpCooldownTimer = jumpCooldown;
        }
    }
}