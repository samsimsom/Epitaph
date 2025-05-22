using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Stamina : ICondition
    {
        public event Action OnStaminaDepleted;
        public event Action OnStaminaRecoveryStarted;
        public event Action OnStaminaRecoveryFinished;

        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float BaseIncreaseRate { get; set; } // Regeneration rate
        public float BaseDecreaseRate { get; set; } // Consumption rate
        public float Modifier { get; set; } = 1f;   // General modifier (e.g., for faster consumption or recovery)

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        public float EffectiveDecreaseRate => BaseDecreaseRate * Modifier;

        public float RecoveryDelay { get; set; }
        public float EnoughPercentage { get; set; } // Percentage of stamina to be considered "enough" (not currently used, but was a field)

        private bool _isConsuming; // Internal state, not directly IsSprinting
        private bool _hasFinishedRecovery;

        private CancellationTokenSource _sprintCts;
        private CancellationTokenSource _recoveryCts;

        // Consider if IsSprinting should be managed externally or if Start/Stop methods are sufficient.
        // For now, keeping it as is, tied to internal _isConsuming.
        public bool IsConsuming => _isConsuming;


        public Stamina(float initialValue, float maxValue, float baseIncreaseRate, float baseDecreaseRate, float recoveryDelay, float enoughPercentage = 0.25f)
        {
            MaxValue = maxValue;
            Value = Mathf.Clamp(initialValue, 0, MaxValue);
            BaseIncreaseRate = baseIncreaseRate;
            BaseDecreaseRate = baseDecreaseRate;
            RecoveryDelay = recoveryDelay;
            EnoughPercentage = enoughPercentage;
        }

        public void StartStaminaConsuming()
        {
            _isConsuming = true; // Mark as consuming
            _recoveryCts?.Cancel();
            _recoveryCts?.Dispose();
            _recoveryCts = null;
            
            if (_sprintCts == null || _sprintCts.IsCancellationRequested)
            {
                _sprintCts = new CancellationTokenSource();
                StaminaConsumeAsync(_sprintCts.Token).Forget();
            }
        }

        public void StopStaminaConsuming()
        {
            _isConsuming = false; // Mark as not consuming
            _sprintCts?.Cancel();
            _sprintCts?.Dispose();
            _sprintCts = null;

            if (Value < MaxValue) // Only start recovery if not already full
            {
                if (_recoveryCts == null || _recoveryCts.IsCancellationRequested)
                {
                    _recoveryCts = new CancellationTokenSource();
                    StaminaRecoveryWithDelay(_recoveryCts.Token).Forget();
                }
            }
        }

        private async UniTaskVoid StaminaConsumeAsync(CancellationToken token)
        {
            // _isConsuming is already set true by StartStaminaConsuming
            _hasFinishedRecovery = false;

            while (!token.IsCancellationRequested && _isConsuming) // Check _isConsuming as well, in case Stop is called externally without IsSprinting setter
            {
                Decrease(EffectiveDecreaseRate * Time.deltaTime);
                if (Value <= 0)
                {
                    OnStaminaDepleted?.Invoke();
                    // _isConsuming should be false now as we can't consume further.
                    // StopStaminaConsuming might be called by the system listening to OnStaminaDepleted.
                    break; 
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
             // If loop exited due to cancellation or Value <=0, ensure _isConsuming reflects state if not already handled.
            if (Value <=0) _isConsuming = false;
        }

        private async UniTaskVoid StaminaRecoveryWithDelay(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(RecoveryDelay), cancellationToken: token);

            if (token.IsCancellationRequested || _isConsuming) return; // Don't recover if consuming again

            OnStaminaRecoveryStarted?.Invoke();
            _hasFinishedRecovery = false;

            await StaminaIncreaseAsync(token);

            // Check if truly finished, Value might not be exactly MaxValue due to float precision
            if (Value >= MaxValue - float.Epsilon) 
            {
                Value = MaxValue; // Clamp to max
                if (!_hasFinishedRecovery)
                {
                    OnStaminaRecoveryFinished?.Invoke();
                    _hasFinishedRecovery = true;
                }
            }
        }

        private async UniTask StaminaIncreaseAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && Value < MaxValue && !_isConsuming) // Stop if consuming starts
            {
                Increase(EffectiveIncreaseRate * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        public void Increase(float amount)
        {
            Value = Mathf.Clamp(Value + amount, 0, MaxValue);
        }

        public void Decrease(float amount)
        {
            Value = Mathf.Clamp(Value - amount, 0, MaxValue);
            if (Value <= 0)
            {
                 Value = 0; // Ensure it's exactly 0 if depleted
                // OnStaminaDepleted could be invoked here or in ConsumeAsync
            }
        }

        public void UpdateCondition(float deltaTime)
        {
            // Stamina is actively managed by Consume/Recovery async methods,
            // so passive UpdateStat is not typically used.
        }
    }
}