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

        public float BaseIncreaseRate { get; set; }
        public float BaseDecreaseRate { get; set; }
        public float Modifier { get; set; } = 1f;

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        public float EffectiveDecreaseRate => BaseDecreaseRate * Modifier;

        public float RecoveryDelay { get; set; } = 3.0f;
        public float EnoughPercentage { get; set; } = 0.25f;

        private bool _isConsuming;
        private bool _hasFinishedRecovery;

        private CancellationTokenSource _sprintCts;
        private CancellationTokenSource _recoveryCts;

        public bool IsSprinting
        {
            get => _isConsuming;
            set => _isConsuming = value;
        }

        public Stamina(float max, float increaseRate, float decreaseRate)
        {
            MaxValue = max;
            Value = max;
            BaseIncreaseRate = increaseRate;
            BaseDecreaseRate = decreaseRate;
        }

        public void StartStaminaConsuming()
        {
            // Sprint başlatıldı, önce varsa toparlanmayı ve eski sprinti iptal et
            if (_recoveryCts != null && !_recoveryCts.IsCancellationRequested)
            {
                _recoveryCts.Cancel();
                _recoveryCts.Dispose();
                _recoveryCts = null;
            }
            if (_sprintCts == null || _sprintCts.IsCancellationRequested)
            {
                _sprintCts = new CancellationTokenSource();
                StaminaConsumeAsync(_sprintCts.Token).Forget();
            }
        }

        public void StopStaminaConsuming()
        {
            // Sprint bırakıldı, hemen stamina doldurma başlatma! Önce delay başlat.
            if (_sprintCts != null && !_sprintCts.IsCancellationRequested)
            {
                _sprintCts.Cancel();
                _sprintCts.Dispose();
                _sprintCts = null;
            }
            if (_recoveryCts == null || _recoveryCts.IsCancellationRequested)
            {
                _recoveryCts = new CancellationTokenSource();
                StaminaRecoveryWithDelay(_recoveryCts.Token).Forget();
            }
        }

        private async UniTaskVoid StaminaConsumeAsync(CancellationToken token)
        {
            _isConsuming = true;
            _hasFinishedRecovery = false;

            while (!token.IsCancellationRequested)
            {
                Decrease(EffectiveDecreaseRate * Time.deltaTime);
                if (Value <= 0)
                {
                    _isConsuming = false;
                    OnStaminaDepleted?.Invoke();
                    break;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            _isConsuming = false;
        }

        private async UniTaskVoid StaminaRecoveryWithDelay(CancellationToken token)
        {
            // Staminanın tekrar dolmaya başlaması için delay
            await UniTask.Delay(TimeSpan.FromSeconds(RecoveryDelay), cancellationToken: token);

            if (token.IsCancellationRequested) return;

            OnStaminaRecoveryStarted?.Invoke();
            _hasFinishedRecovery = false;

            await StaminaIncreaseAsync(token);

            if (Mathf.Approximately(Value, MaxValue))
            {
                if (!_hasFinishedRecovery)
                {
                    OnStaminaRecoveryFinished?.Invoke();
                    _hasFinishedRecovery = true;
                }
            }
        }

        private async UniTask StaminaIncreaseAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && Value < MaxValue)
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
        }

        public void UpdateStat(float deltaTime)
        {
            // Bu kısıma "otomatik dolum" yazmana gerek yok, sistem Consume/Recovery ile yönetiliyor
        }
    }
}