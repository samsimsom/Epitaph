using System;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerSprint : MonoBehaviour
    {
        public static event Action<float> OnChangeSprintSpeed;
        
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        [SerializeField] private PlayerCondition playerCondition;

        [Header("Debug")]
        // [SerializeField] private float currentStamina;
        [SerializeField] private bool canSprint = true;
        
        private float _timeSinceLastSprint;
        private bool _isSprintKeyHeld;
        
        private StaminaCondition Stamina => playerCondition ? playerCondition.Stamina : null;

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

        private void Update()
        {
            if (_isSprintKeyHeld && !playerMovementData.isSprinting)
            {
                TryStartSprint();
            }
            
            UpdateStamina();
        }

        private void Initialize()
        {
            if (!playerCondition) playerCondition = GetComponent<PlayerCondition>();
        }

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
            if (!playerMovementData.isGrounded || !canSprint || !(Stamina?.Value > 0)) return;
            if (playerMovementData.isCrouching) return;

            playerMovementData.isSprinting = true;
            playerCondition.SetRunning(true); // hunger, thirst modifikasyonu için
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
            if (Stamina == null) return;

            // Sprint sırasında stamina harca
            if (playerMovementData.isSprinting)
            {
                Stamina.Decrease(playerMovementData.sprintStaminaUsage * Time.deltaTime);
                _timeSinceLastSprint = 0f;

                if (Stamina.Value <= 0)
                {
                    canSprint = false;
                    StopSprint();
                }
            }
            else
            {
                _timeSinceLastSprint += Time.deltaTime;

                if (_timeSinceLastSprint >= playerMovementData.staminaRecoveryDelay)
                {
                    Stamina.Increase(playerMovementData.staminaRecoveryRate * Time.deltaTime);

                    if (Stamina.Value > Stamina.MaxValue * playerMovementData.staminaEnoughPercentage)
                    {
                        canSprint = true;
                    }
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