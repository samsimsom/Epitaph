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
        
        public float RecoveryDelay { get; set; } = 3.0f; // Koşma sonrası gecikme
        public float EnoughPercentage { get; set; } = 0.25f;

        private bool _isConsuming;
        private float _recoveryTimer;
        private bool _hasStartedRecovery;
        private bool _hasFinishedRecovery;

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
            _hasStartedRecovery = false;
            _hasFinishedRecovery = false;
        }

        public void StopConsumingSprint()
        {
            _isConsuming = false;
            _recoveryTimer = 0f;
            // Recovery başladığında event tetiklenebilmesi için flag'leri sıfırla
            _hasStartedRecovery = false;
            _hasFinishedRecovery = false;
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
            if (_isConsuming)
            {
                Value = Mathf.Clamp(Value - EffectiveDecreaseRate * deltaTime, 0, MaxValue);
                if (Value <= 0)
                {
                    _isConsuming = false; // Otomatik durdur
                    OnStaminaDepleted?.Invoke();
                }
                _recoveryTimer = 0f;
                _hasStartedRecovery = false;
                _hasFinishedRecovery = false;
            }
            else
            {
                _recoveryTimer += deltaTime;
                if (_recoveryTimer >= RecoveryDelay)
                {
                    Value = Mathf.Clamp(Value + EffectiveIncreaseRate * deltaTime, 0, MaxValue);

                    // 'Value == MaxValue' durumunda recovery tamamlandı:
                    if (Mathf.Approximately(Value, MaxValue))
                    {
                        if (!_hasFinishedRecovery)
                        {
                            OnStaminaRecoveryFinished?.Invoke();
                            _hasFinishedRecovery = true;
                            _hasStartedRecovery = false;
                        }
                    }
                    // 'Value >= MaxValue * EnoughPercentage' (ama henüz MaxValue olmadı!) recovery başladı:
                    else if (Value >= MaxValue * EnoughPercentage)
                    {
                        if (!_hasStartedRecovery)
                        {
                            OnStaminaRecoveryStarted?.Invoke();
                            _hasStartedRecovery = true;
                            _hasFinishedRecovery = false;
                        }
                    }
                    else
                    {
                        // Recovery yeterli seviyede başlamadıysa flagleri sıfırla
                        _hasStartedRecovery = false;
                        _hasFinishedRecovery = false;
                    }
                }
                else
                {
                    // Recovery daha başlamadıysa flag'leri sıfırla
                    _hasStartedRecovery = false;
                    _hasFinishedRecovery = false;
                }
            }
        }
    }
}