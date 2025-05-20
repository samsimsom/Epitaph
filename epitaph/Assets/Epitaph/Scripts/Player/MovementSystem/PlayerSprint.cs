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
            
            playerCondition.Stamina.OnStaminaDepleted -= OnStaminaDepleted;
            playerCondition.Stamina.OnStaminaRecoveryStarted -= OnRecoveryStarted;
            playerCondition.Stamina.OnStaminaRecoveryFinished -= OnRecoveryFinished;
        }

        private void Start()
        {
            playerCondition.Stamina.OnStaminaDepleted += OnStaminaDepleted;
            playerCondition.Stamina.OnStaminaRecoveryStarted += OnRecoveryStarted;
            playerCondition.Stamina.OnStaminaRecoveryFinished += OnRecoveryFinished;
        }

        private void Update()
        {
            // Başlat işlemi hâlâ burada (Kısayol desteği için)
            if (_isSprintKeyHeld && !playerMovementData.isSprinting)
            {
                TryStartSprint();
            }
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
        
        private void OnStaminaDepleted()
        {
            StopSprint();
            // Debug.Log("Sprint recovery depleted");
        }
        
        private void OnRecoveryStarted()
        {
            // Debug.Log("Sprint recovery started");
        }
        
        private void OnRecoveryFinished()
        {
            // Debug.Log("Sprint recovery finished");
        }
        
        private void TryStartSprint()
        {
            if (!playerMovementData.isGrounded || 
                playerCondition?.Stamina == null ||
                playerCondition.Stamina.Value <= 0) return;
            
            if (playerMovementData.isCrouching) return;

            playerMovementData.isSprinting = true;
            playerCondition.SetRunning(true); // hunger, thirst modifikasyon için
            playerCondition.Stamina.StartStaminaConsuming();
            OnChangeSprintSpeed?.Invoke(playerMovementData.sprintSpeed);
        }

        private void StopSprint()
        {
            if (!playerMovementData.isSprinting) return;
            
            playerMovementData.isSprinting = false;
            playerCondition.SetRunning(false); // hunger, thirst eski haline döner
            playerCondition.Stamina.StopStaminaConsuming();
            OnChangeSprintSpeed?.Invoke(playerMovementData.walkSpeed);
        }
        
        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (!isCrouching || !playerMovementData.isSprinting) return;
            
            StopSprint();
            OnChangeSprintSpeed?.Invoke(playerMovementData.crouchSpeed);
        }
        
    }
}