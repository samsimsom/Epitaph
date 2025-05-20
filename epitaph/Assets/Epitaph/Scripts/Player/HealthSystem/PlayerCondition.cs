using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class PlayerCondition : PlayerBehaviour
    {
        public PlayerCondition(PlayerController playerController, 
            PlayerMovementData playerMovementData) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            InitializeConditions();
        }
        
        #region Public Properties
        public Health Health
        {
            get => _health;
            private set => _health = value;
        }
        public Stamina Stamina
        {
            get => _stamina;
            private set => _stamina = value;
        }
        public Hunger Hunger
        {
            get => _hunger;
            private set => _hunger = value;
        }
        public Thirst Thirst
        {
            get => _thirst;
            private set => _thirst = value;
        }
        public Fatigue Fatigue
        {
            get => _fatigue;
            private set => _fatigue = value;
        }
        #endregion

        #region Private Fields

        private PlayerMovementData _playerMovementData;
        
        private Health _health;
        private Stamina _stamina;
        private Hunger _hunger;
        private Thirst _thirst;
        private Fatigue _fatigue;
        private List<ICondition> _allStats;
        
        private int _lastMinute = -1;
        private int _lastSecond = -1;
        
        private bool _isUpdating;
        #endregion

        #region Lifecycle Methods
        public override void Start()
        {
            StartUpdates();
        }

        public override void OnEnable()
        {
            _isUpdating = true;
            
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped += OnTimeSkipped;
            }
            else
            {
                Debug.LogWarning("GameTime instance not found. Player conditions will not update on time skip.");
            }

        }

        public override void OnDisable()
        {
            _isUpdating = false;
            
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped -= OnTimeSkipped;
            }
        }

        public override void Update()
        {
            
        }
        #endregion
        
        private void InitializeConditions()
        {
            Health = new Health(100f, 1.0f);
            Stamina = new Stamina(100f, 1f, 1f);
            Hunger = new Hunger(100f, 0.1f);
            Thirst = new Thirst(100f, 0.3f);
            Fatigue = new Fatigue(100f, 0.1f);
            
            _allStats = new List<ICondition> { Health, Stamina, Hunger, Thirst, Fatigue };
        }
        
        private void StartUpdates()
        {
            FrameBasedUpdates().Forget();
            // SecondBasedUpdates().Forget();
            MinuteBasedUpdates().Forget();
        }
        
        #region Update Methods
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (_isUpdating)
            {
                // Update frame-sensitive stats
                var delta = Time.deltaTime;
                Health.UpdateStat(delta);
                Stamina.UpdateStat(delta);
                
                _playerMovementData.health = Health.Value;
                // _playerMovementData.maxHealth = Health.MaxValue;
                _playerMovementData.stamina = Stamina.Value;
                // _playerMovementData.maxStamina = Stamina.MaxValue;
                _playerMovementData.hunger = Hunger.Value;
                // _playerMovementData.maxHunger = Hunger.MaxValue;
                _playerMovementData.thirst = Thirst.Value;
                // _playerMovementData.maxThirst = Thirst.MaxValue;
                _playerMovementData.fatigue = Fatigue.Value;
                // _playerMovementData.maxFatigue = Fatigue.MaxValue;
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        private async UniTaskVoid SecondBasedUpdates()
        {
            while (_isUpdating)
            {
                // Check for second change
                var currentSecond = GameTime.Instance.GameSecond;
                if (currentSecond != _lastSecond)
                {
                    _lastSecond = currentSecond;
                    UpdateNonVitalStats(1.0f);
                }

                // Wait for next game second to complete
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private async UniTaskVoid MinuteBasedUpdates()
        {
            while (_isUpdating)
            {
                // Check for minute change
                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    UpdateNonVitalStats(1.0f);
                }
                
                // Wait for next game second to complete
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private void UpdateNonVitalStats(float amount)
        {
            foreach (var stat in _allStats)
            {
                if (stat != Health && stat != Stamina)
                    stat.UpdateStat(amount);
            }
        }
        #endregion

        #region Public Interaction Methods
        public void Eat(float amount) => Hunger.Decrease(amount);
        public void Drink(float amount) => Thirst.Decrease(amount);
        public void Sleep(float hours) => Fatigue.Decrease(hours * 20f);
        #endregion

        #region Condition Modifiers
        public void SetRunning(bool isRunning)
        {
            // Running increases hunger by 20%, thirst by 60%
            Hunger.Modifier = isRunning ? 1.2f : 1f;
            Thirst.Modifier = isRunning ? 1.6f : 1f;
        }
        
        public void SetOutsideTemperature(float temperature)
        {
            // Heat increases thirst rate
            Thirst.Modifier = temperature > 35f ? 1.5f : 1f;
        }
        
        public void OnTimeChanged(int gameHour)
        {
            // Fatigue increases faster at night
            Fatigue.Modifier = (gameHour >= 22 || gameHour <= 6) ? 1.4f : 1f;
        }
        #endregion
        
        #region Time Skip Handler
        private void OnTimeSkipped(float hoursSkipped)
        {
            Debug.Log($"Time skipped: {hoursSkipped} hours. Updating player conditions...");
    
            // Atlanılan dakikaları hesapla (şu anki mantığa göre her dakika için 1.0f etki uygulanıyor)
            var minutesSkipped = hoursSkipped * 60f;
    
            // Her non-vital koşul için atlanılan dakikalara göre güncelleme yap
            foreach (var stat in _allStats)
            {
                // Health ve Stamina dışındaki koşulları güncelle (açlık, susuzluk, yorgunluk)
                if (stat != Health && stat != Stamina)
                {
                    // Her dakika için 1.0f değerinde bir etki ekleniyor (MinuteBasedUpdates ile aynı mantık)
                    stat.UpdateStat(minutesSkipped);
            
                    // Güncellenmiş her koşul için debug log ekleyebilirsiniz
                    Debug.Log($"Updated {stat.GetType().Name} after time skip. New value: {stat.Value}");
                }
            }
        }
        #endregion
        
    }
}