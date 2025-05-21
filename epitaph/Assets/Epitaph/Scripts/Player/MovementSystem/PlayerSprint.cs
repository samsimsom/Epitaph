using System;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerSprint : PlayerBehaviour
    {
        public static event Action<float> OnChangeSprintSpeed;

        private PlayerData _playerData;
        private PlayerCondition _playerCondition;
        private PlayerMove _playerMove;
        
        private bool _isSprintKeyHeld; // This field is declared but never used.

        public PlayerSprint(PlayerController playerController, 
            PlayerData playerData,
            PlayerCondition playerCondition,
            PlayerMove playerMove) : base(playerController)
        {
            _playerData = playerData;
            _playerCondition = playerCondition;
            _playerMove = playerMove;
        }

        public override void OnEnable()
        {
            PlayerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
            
            _playerCondition.Stamina.OnStaminaDepleted += OnStaminaDepleted;
            _playerCondition.Stamina.OnStaminaRecoveryStarted += OnRecoveryStarted;
            _playerCondition.Stamina.OnStaminaRecoveryFinished += OnRecoveryFinished;
        }

        public override void OnDisable()
        {
            PlayerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
            
            _playerCondition.Stamina.OnStaminaDepleted -= OnStaminaDepleted;
            _playerCondition.Stamina.OnStaminaRecoveryStarted -= OnRecoveryStarted;
            _playerCondition.Stamina.OnStaminaRecoveryFinished -= OnRecoveryFinished;
        }
        
        public void TryStartSprint()
        {
            if (!_playerData.isGrounded || 
                PlayerController.GetPlayerCondition()?.Stamina == null ||
                PlayerController.GetPlayerCondition().Stamina.Value <= 0) return;
            
            if (_playerData.isCrouching) return;
            
            _playerData.isSprinting = true;
            _playerMove.SetRunningSpeed();
            PlayerController.GetPlayerCondition().SetRunning(_playerData.isSprinting); // hunger, thirst modifikasyon için
            PlayerController.GetPlayerCondition().Stamina.StartStaminaConsuming();
            OnChangeSprintSpeed?.Invoke(_playerData.sprintSpeed);
        }

        public void StopSprint()
        {
            if (!_playerData.isSprinting) return;
            
            _playerData.isSprinting = false;
            _playerMove.SetWalkingSpeed();
            PlayerController.GetPlayerCondition().SetRunning(_playerData.isSprinting); // hunger, thirst eski haline döner
            PlayerController.GetPlayerCondition().Stamina.StopStaminaConsuming();
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