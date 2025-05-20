using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class PlayerCondition : MonoBehaviour
    {
        #region Public Properties
        public Health Health { get; private set; }
        public Stamina Stamina { get; private set; }
        public Hunger Hunger { get; private set; }
        public Thirst Thirst { get; private set; }
        public Fatigue Fatigue { get; private set; }
        #endregion

        #region Private Fields
        private List<ICondition> _allStats;
        private int _lastMinute = -1;
        private int _lastSecond = -1;
        #endregion

        #region Debug Fields
        [Header("Debug Values")]
        [SerializeField] private float health;
        [SerializeField] private float stamina;
        [Space]
        [SerializeField] private float hunger;
        [SerializeField] private float thirst;
        [SerializeField] private float fatigue;
        #endregion

        #region Event Definitions (Commented)
        // public static event Action<float, float> OnHealthChanged;
        // public static event Action<float, float> OnStaminaChanged;
        // public static event Action<float, float> OnHungerChanged;
        // public static event Action<float, float> OnThirstChanged;
        // public static event Action<float, float> OnFatigueChanged;
        // public static event Action OnDie;
        #endregion

        #region Lifecycle Methods
        private void Awake()
        {
            InitializeConditions();
            StartUpdates();
        }

        private void OnEnable()
        {
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped += OnTimeSkipped;
            }
            else
            {
                Debug.LogWarning("GameTime instance not found. Player conditions will not update on time skip.");
            }

        }

        private void OnDisable()
        {
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped -= OnTimeSkipped;
            }
        }

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
        #endregion

        #region Update Methods
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                // Update frame-sensitive stats
                var delta = Time.deltaTime;
                Health.UpdateStat(delta);
                Stamina.UpdateStat(delta);

                // Update debug values
                health = Health.Value;
                stamina = Stamina.Value;
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        private async UniTaskVoid SecondBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                // Check for second change
                var currentSecond = GameTime.Instance.GameSecond;
                if (currentSecond != _lastSecond)
                {
                    _lastSecond = currentSecond;
                    UpdateNonVitalStats(1.0f);
                }
                
                UpdateInspectorData();
                
                // Wait for next game second to complete
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private async UniTaskVoid MinuteBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                // Check for minute change
                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    UpdateNonVitalStats(1.0f);
                }
                
                UpdateInspectorData();
                
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
    
            // Debug değerlerini güncelle
            UpdateInspectorData();
        }
        #endregion

        #region Debug Methods
        private void UpdateInspectorData()
        {
            hunger = Hunger.Value;
            thirst = Thirst.Value;
            fatigue = Fatigue.Value;
        }
        #endregion
    }
}