using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerSprint : MonoBehaviour
    {
        public static event Action<float> OnChangeSprintSpeed;
        
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;

        [Header("Debug")]
        [SerializeField] private float currentStamina;
        [SerializeField] private bool canSprint = true;
        
        private float _timeSinceLastSprint;
        private bool _isSprintKeyHeld;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            PlayerInput.OnSprintActivated += OnSprintActivated;
            PlayerInput.OnSprintDeactivated += OnSprintDeactivated;
            PlayerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
        }

        private void OnDisable()
        {
            PlayerInput.OnSprintActivated -= OnSprintActivated;
            PlayerInput.OnSprintDeactivated -= OnSprintDeactivated;
            PlayerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
        }
        
        private void Start()
        {
            currentStamina = playerMovementData.maxStamina;
        }

        private void Update()
        {
            if (_isSprintKeyHeld && !playerMovementData.isSprinting)
            {
                TryStartSprint();
            }
            
            UpdateStamina();
        }

        private void Initialize() { }

        private void OnSprintActivated()
        {
            // Start sprint when button is pressed
            _isSprintKeyHeld = true;
            TryStartSprint();
        }

        private void OnSprintDeactivated()
        {
            // Stop sprint when button is released
            _isSprintKeyHeld = false;
            StopSprint();
        }
        
        private void TryStartSprint()
        {
            // Eğer oyuncu yerdeyse ve sprint yapabiliyorsa
            if (!playerMovementData.isGrounded || !canSprint || !(currentStamina > 0)) return;
            
            // Çömelmiyorsa
            if (playerMovementData.isCrouching) return;
            
            playerMovementData.isSprinting = true;
            OnChangeSprintSpeed?.Invoke(playerMovementData.sprintSpeed);
        }

        private void StopSprint()
        {
            if (!playerMovementData.isSprinting) return;
            
            playerMovementData.isSprinting = false;
            OnChangeSprintSpeed?.Invoke(playerMovementData.walkSpeed);
        }

        private void UpdateStamina()
        {
            // If sprinting, reduce stamina
            if (playerMovementData.isSprinting)
            {
                currentStamina -= playerMovementData.sprintStaminaUsage * Time.deltaTime;
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

                if (!(_timeSinceLastSprint >= playerMovementData.staminaRecoveryDelay)) return;
                
                currentStamina += playerMovementData.staminaRecoveryRate * Time.deltaTime;
                    
                // If stamina is recovered enough, allow sprinting again
                if (currentStamina > playerMovementData.maxStamina * playerMovementData.staminaEnoughPercentage)
                {
                    canSprint = true;
                }
                    
                // Cap stamina at max
                if (currentStamina > playerMovementData.maxStamina)
                {
                    currentStamina = playerMovementData.maxStamina;
                }
            }
            
        }
        
        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (!isCrouching || !playerMovementData.isSprinting) return;
            
            StopSprint();
            OnChangeSprintSpeed?.Invoke(playerMovementData.crouchSpeed);
        }
        
    }
}