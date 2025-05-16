// ReSharper disable CommentTypo, IdentifierTypo, GrammarMistakeInComment
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Epitaph.Scripts.Player
{
    [RequireComponent(typeof(PlayerMove))]
    public class PlayerSprint : MonoBehaviour
    {
        [FormerlySerializedAs("playerMovement")]
        [Header("References")]
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private PlayerCrouch playerCrouch;
        [SerializeField] private PlayerGravity playerGravity;

        [Header("Sprint Settings")]
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float sprintStaminaUsage = 10f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRecoveryRate = 20f;
        [SerializeField] private float staminaRecoveryDelay = 1f;

        [Header("Debug")]
        [SerializeField] private bool isSprinting;
        [SerializeField] private float currentStamina;
        [SerializeField] private bool canSprint = true;

        private float _defaultMoveSpeed;
        private float _timeSinceLastSprint;
        private bool _isSprintKeyHeld;

        private void Awake()
        {
            InitializeComponents();
        }

        private void OnEnable()
        {
            if (playerCrouch != null)
            {
                playerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
            }
        }

        private void OnDisable()
        {
            if (playerCrouch != null)
            {
                playerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
            }
        }
        
        private void Start()
        {
            // _defaultMoveSpeed = playerMove.GetMoveSpeed();
            currentStamina = maxStamina;
        }

        private void Update()
        {
            if (_isSprintKeyHeld && !isSprinting)
            {
                // Yerdeyiz, sprint tuşu basılı ve sprint yapmıyoruz
                // Sprint'i tekrar başlatmayı dene
                TryStartSprint();
            }
            
            // Stamina güncellemelerini yap
            UpdateStamina();
        }

        private void InitializeComponents()
        {
            if (playerMove == null)
            {
                playerMove = GetComponent<PlayerMove>();
            }

            if (playerCrouch == null)
            {
                playerCrouch = GetComponent<PlayerCrouch>();
            }
            
            if (playerGravity == null)
            {
                playerGravity = GetComponent<PlayerGravity>();
            }
        }

        public void OnSprintPerformed(InputAction.CallbackContext context)
        {
            // Start sprint when button is pressed
            if (context.performed)
            {
                _isSprintKeyHeld = true;
                TryStartSprint();
            }
            // Stop sprint when button is released
            else if (context.canceled)
            {
                _isSprintKeyHeld = false;
                StopSprint();
            }
        }
        
        private void TryStartSprint()
        {
            // Eğer oyuncu yerdeyse ve sprint yapabiliyorsa
            if (playerGravity == null || !playerGravity.IsGrounded() 
                                      || !canSprint || !(currentStamina > 0)) return;
            
            // Çömelmiyorsa
            if (playerCrouch != null && playerCrouch.IsCrouching()) return;
            
            isSprinting = true;
            // playerMove.SetMoveSpeed(sprintSpeed);
        }

        private void StopSprint()
        {
            if (!isSprinting) return;
            
            isSprinting = false;
            // playerMove.SetMoveSpeed(_defaultMoveSpeed);
        }

        private void UpdateStamina()
        {
            // If sprinting, reduce stamina
            if (isSprinting)
            {
                currentStamina -= sprintStaminaUsage * Time.deltaTime;
                _timeSinceLastSprint = 0f;
                
                // If stamina is depleted, stop sprinting
                if (!(currentStamina <= 0)) return;
                
                currentStamina = 0;
                canSprint = false;
                StopSprint();
            }
            // If not sprinting, recover stamina after delay
            else
            {
                _timeSinceLastSprint += Time.deltaTime;

                if (!(_timeSinceLastSprint >= staminaRecoveryDelay)) return;
                
                currentStamina += staminaRecoveryRate * Time.deltaTime;
                    
                // If stamina is recovered enough, allow sprinting again
                if (currentStamina > maxStamina * 0.15f)
                {
                    canSprint = true;
                }
                    
                // Cap stamina at max
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
            
        }
        
        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (!isCrouching || !isSprinting) return;
            
            StopSprint();
            // playerMove.SetMoveSpeed(playerCrouch.GetCrouchSpeed());
        }
        
        public bool IsSprinting()
        {
            return isSprinting;
        }

        public float GetStaminaPercentage()
        {
            return currentStamina / maxStamina;
        }
        
    }
}