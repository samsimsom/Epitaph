using System;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerSprint : PlayerBehaviour
    {
        public PlayerSprint(PlayerController playerController, 
            PlayerMovementData playerMovementData, 
            PlayerCondition playerCondition) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            _playerCondition = playerCondition;
        }

        public static event Action<float> OnChangeSprintSpeed;
        
        private PlayerMovementData _playerMovementData;
        private PlayerCondition _playerCondition;
        
        
        private bool _isSprintKeyHeld;

        public override void OnEnable()
        {
            PlayerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
        }

        public override void OnDisable()
        {
            PlayerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
            
            _playerCondition.Stamina.OnStaminaDepleted -= OnStaminaDepleted;
            _playerCondition.Stamina.OnStaminaRecoveryStarted -= OnRecoveryStarted;
            _playerCondition.Stamina.OnStaminaRecoveryFinished -= OnRecoveryFinished;
        }

        public override void Start()
        {
            _playerCondition.Stamina.OnStaminaDepleted += OnStaminaDepleted;
            _playerCondition.Stamina.OnStaminaRecoveryStarted += OnRecoveryStarted;
            _playerCondition.Stamina.OnStaminaRecoveryFinished += OnRecoveryFinished;
        }

        public override void Update()
        {
            // Başlat işlemi hâlâ burada (Kısayol desteği için)
            if (_isSprintKeyHeld && !_playerMovementData.isSprinting)
            {
                TryStartSprint();
            }
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
        
        public void TryStartSprint()
        {
            // if (!_playerMovementData.isGrounded || 
            //     PlayerController.GetPlayerCondition()?.Stamina == null ||
            //     PlayerController.GetPlayerCondition().Stamina.Value <= 0) return;
            //
            // if (_playerMovementData.isCrouching) return;
            //
            // _playerMovementData.isSprinting = true;
            // PlayerController.GetPlayerCondition().SetRunning(true); // hunger, thirst modifikasyon için
            // PlayerController.GetPlayerCondition().Stamina.StartStaminaConsuming();
            // OnChangeSprintSpeed?.Invoke(_playerMovementData.sprintSpeed);
        }

        public void StopSprint()
        {
            // if (!_playerMovementData.isSprinting) return;
            //
            // _playerMovementData.isSprinting = false;
            // PlayerController.GetPlayerCondition().SetRunning(false); // hunger, thirst eski haline döner
            // PlayerController.GetPlayerCondition().Stamina.StopStaminaConsuming();
            // OnChangeSprintSpeed?.Invoke(_playerMovementData.walkSpeed);
        }
        
        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (!isCrouching || !_playerMovementData.isSprinting) return;
            
            StopSprint();
            OnChangeSprintSpeed?.Invoke(_playerMovementData.crouchSpeed);
        }
        
    }
}