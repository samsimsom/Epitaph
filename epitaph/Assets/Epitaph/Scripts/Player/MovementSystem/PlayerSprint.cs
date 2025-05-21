using System;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerSprint : PlayerBehaviour
    {
        public static event Action<float> OnChangeSprintSpeed;

        private PlayerData _playerData;
        private HealthController _healthController;
        private PlayerMove _playerMove;
        
        private bool _isSprintKeyHeld; // This field is declared but never used.

        public PlayerSprint(PlayerController playerController, 
            PlayerData playerData,
            HealthController healthController,
            PlayerMove playerMove) : base(playerController)
        {
            _playerData = playerData;
            _healthController = healthController;
            _playerMove = playerMove;
        }

        public override void OnEnable()
        {
            PlayerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
            
            _healthController.Stamina.OnStaminaDepleted += OnStaminaDepleted;
            _healthController.Stamina.OnStaminaRecoveryStarted += OnRecoveryStarted;
            _healthController.Stamina.OnStaminaRecoveryFinished += OnRecoveryFinished;
        }

        public override void OnDisable()
        {
            PlayerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
            
            _healthController.Stamina.OnStaminaDepleted -= OnStaminaDepleted;
            _healthController.Stamina.OnStaminaRecoveryStarted -= OnRecoveryStarted;
            _healthController.Stamina.OnStaminaRecoveryFinished -= OnRecoveryFinished;
        }
        
        public void TryStartSprint()
        {
            if (!_playerData.isGrounded || 
                _healthController?.Stamina == null ||
                _healthController.Stamina.Value <= 0) return;
            
            if (_playerData.isCrouching) return;
            
            _playerData.isSprinting = true;
            _playerMove.SetRunningSpeed();
            _healthController.SetRunning(_playerData.isSprinting); // hunger, thirst modifikasyon için
            _healthController.Stamina.StartStaminaConsuming();
            OnChangeSprintSpeed?.Invoke(_playerData.sprintSpeed);
        }

        public void StopSprint()
        {
            if (!_playerData.isSprinting) return;
            
            _playerData.isSprinting = false;
            _playerMove.SetWalkingSpeed();
            _healthController.SetRunning(_playerData.isSprinting); // hunger, thirst eski haline döner
            _healthController.Stamina.StopStaminaConsuming();
            OnChangeSprintSpeed?.Invoke(_playerData.walkSpeed);
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
        
        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (!isCrouching || !_playerData.isSprinting) return;
            
            StopSprint();
            _playerMove.SetCrouchingSpeed();
        }
    }
}