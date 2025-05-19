using System;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class StaminaCondition : ICondition
    {
        public event Action OnStaminaDepleted;
        public event Action OnStaminaRecoveryStarted;
        public event Action OnStaminaRecoveryFinished;
        
        public float Value { get; private set;}
        public float MaxValue { get; private set;}
        
        public float BaseIncreaseRate { get; set; }
        public float BaseDecreaseRate { get; set; }
        public float Modifier { get; set; } = 1f;
        
        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        public float EffectiveDecreaseRate => BaseDecreaseRate * Modifier;
        
        public float RecoveryDelay { get; set; } = 1.0f; // Koşma sonrası gecikme
        public float EnoughPercentage { get; set; } = 0.25f;

        private bool _isConsuming;
        private float _recoveryTimer;

        public bool IsSprinting
        {
            get => _isConsuming;
            set => _isConsuming = value;
        }
        
        public StaminaCondition(float max, float increaseRate, float decreaseRate)
        {
            MaxValue = max;
            Value = max;
            BaseIncreaseRate = increaseRate;
            BaseDecreaseRate = decreaseRate;
        }
        
        public void StartConsumingSprint()
        {
            _isConsuming = true;
            _recoveryTimer = 0f;
        }

        public void StopConsumingSprint()
        {
            _isConsuming = false;
            _recoveryTimer = 0f; // Recovery'yi başlat
        }
        
        public void Increase(float amount)
        {
            Value = Mathf.Clamp(Value + amount, 0, MaxValue);
        }

        public void Decrease(float amount)
        {
            Value = Mathf.Clamp(Value - amount, 0, MaxValue);
        }

        public void UpdateStat(float deltaTime)
        {
            Debug.Log($"Stamina: {Value}");
            if (_isConsuming)
            {
                Value = Mathf.Clamp(Value - EffectiveDecreaseRate * deltaTime, 0, MaxValue);
                if (Value <= 0)
                {
                    OnStaminaDepleted?.Invoke();
                    _isConsuming = false; // Otomatik durdur
                    return;
                }
                _recoveryTimer = 0f;
            }
            else
            {
                _recoveryTimer += deltaTime;
                if (_recoveryTimer >= RecoveryDelay)
                {
                    Value = Mathf.Clamp(Value + EffectiveIncreaseRate * deltaTime, 0, MaxValue);
                    if (Value >= MaxValue * EnoughPercentage)
                    {
                        OnStaminaRecoveryFinished?.Invoke();
                        return;
                    }
                    OnStaminaRecoveryStarted?.Invoke();
                }
            }
        }

    }
}