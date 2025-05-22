using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class HealthController : PlayerBehaviour
    {
        private PlayerController _playerController;
        private PlayerData _playerData;

        public Health Health { get; private set; }
        public Stamina Stamina { get; private set; }
        public Hunger Hunger { get; private set; }
        public Thirst Thirst { get; private set; }
        public Fatigue Fatigue { get; private set; }
        
        private List<ICondition> _allStats;
        private int _lastMinute = -1;
        private bool _isUpdating;
        
        public HealthController(PlayerController playerController, PlayerData playerData) 
            : base(playerController)
        {
            _playerData = playerData;
        }

        public override void Awake()
        {
            InitializeConditions();
        }
        
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
                // Assuming GameTime also fires an event for regular time changes if needed for OnTimeChanged
                // For now, OnTimeChanged is called externally or through a different mechanism.
            }
            else
            {
                Debug.LogWarning("GameTime instance not found. " +
                                 "Player conditions will not update on time skip.");
            }
        }

        public override void OnDisable()
        {
            _isUpdating = false; 
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped -= OnTimeSkipped;
            }
            // Important: Also cancel any running UniTasks in Stamina if HealthController itself is disabled/destroyed
            // Stamina's CancellationTokenSources should ideally be linked to _isUpdating or a token from HealthController
            // For simplicity here, assuming Stamina handles its cancellations if its owner becomes inactive.
            // A more robust solution would pass a CancellationToken from HealthController to Stamina.
        }
        public void PlayerOnDestroy() { /* Cleanup if necessary */ }

#if UNITY_EDITOR
        public override void OnDrawGizmos() { }
#endif
        
        private void InitializeConditions()
        {
            // Values should come from _playerData (e.g., _playerData.healthConfig.initialValue)
            // Using placeholder values here for demonstration if _playerData structure is unknown.
            // Replace these with actual _playerData fields.

            // Example: _playerData might have distinct config objects or just loose fields.
            // float healthInitial = _playerData.healthInitialValue; float healthMax = _playerData.healthMaxValue; ...

            Health = new Health(
                _playerData.health,
                _playerData.maxHealth,
                _playerData.healthIncreaseRate,
                _playerData.healthDecreaseRate);

            Stamina = new Stamina(
                _playerData.stamina,
                _playerData.maxStamina,
                _playerData.staminaIncreaseRate,
                _playerData.staminaDecreaseRate,
                _playerData.staminaRecoveryDelay,
                _playerData.staminaEnoughPercentage);

            Hunger = new Hunger(
                _playerData.hunger,
                _playerData.maxHunger,
                _playerData.hungerIncreaseRate,
                _playerData.hungerStarvingThreshold);

            Thirst = new Thirst(
                _playerData.thirst,
                _playerData.maxThirst,
                _playerData.thirstIncreaseRate,
                _playerData.thirstDehydrationThreshold);

            Fatigue = new Fatigue(
                _playerData.fatigue,
                _playerData.maxFatigue,
                _playerData.fatiqueIncreaseRate,
                _playerData.fatiqueExhaustionThreshold);
            
            _allStats = new List<ICondition> { Health, Stamina, Hunger, Thirst, Fatigue };
        }
        
        private void StartUpdates()
        {
            FrameBasedUpdates().Forget(); // Handles continuous PlayerData sync
            MinuteBasedUpdates().Forget(); // Handles per-minute stat degradation
        }
        
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (_isUpdating)
            {
                // Health.UpdateStat(Time.deltaTime); // If health had passive regen/degen per frame
                // Stamina is handled by its own async methods.
                
                // Sync current values to PlayerData
                if (_playerData != null)
                {
                    _playerData.health = Health.Value;
                    _playerData.stamina = Stamina.Value;
                    _playerData.hunger = Hunger.Value;
                    _playerData.thirst = Thirst.Value;
                    _playerData.fatigue = Fatigue.Value;
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        private async UniTaskVoid MinuteBasedUpdates()
        {
            while (_isUpdating)
            {
                if (GameTime.Instance == null)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update); 
                    continue;
                }

                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    // Pass 1.0f as deltaTime, signifying one minute has passed.
                    // The BaseIncreaseRate in Hunger, Thirst, Fatigue
                    // should be scaled accordingly (e.g., units per minute).
                    UpdateNonVitalStats(1.0f); 
                }
                
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private void UpdateNonVitalStats(float timeDeltaInMinutes)
        {
            foreach (var stat in _allStats)
            {
                // Health might have its own passive update logic or none.
                // Stamina is managed by its active consumption/recovery cycles.
                if (stat is Hunger || stat is Thirst || stat is Fatigue)
                {
                    stat.UpdateCondition(timeDeltaInMinutes);
                }
            }
        }

        public void Eat(float foodValue) => Hunger.Decrease(foodValue);
        public void Drink(float waterValue) => Thirst.Decrease(waterValue);
        
        public void Sleep(float hoursSlept) 
        {
            // Effectiveness of sleep should be configurable
            var fatigueRecoveryPerHour = _playerData.fatigueRecoveryPerHour;
            Fatigue.Decrease(hoursSlept * fatigueRecoveryPerHour);
        }

        public void SetRunning(bool isRunning)
        {
            // Modifiers should come from PlayerData
            Hunger.Modifier = isRunning ? _playerData.hungerRunningModifier : _playerData.hungerDefaultModifier;
            Thirst.Modifier = isRunning ? _playerData.thirstRunningModifier : _playerData.thirstDefaultModifier;

            // Manage Stamina consumption
            if (isRunning)
            {
                Stamina.StartStaminaConsuming();
            }
            else
            {
                Stamina.StopStaminaConsuming();
            }
        }
        
        public void SetOutsideTemperature(float temperature)
        {
            // float highTempThreshold = _playerData.GetFloat("Thirst.HighTempThreshold", 35f);
            // Thirst.Modifier = temperature > highTempThreshold ? _playerData.GetFloat("Thirst.HighTempModifier", 1.5f) : _playerData.GetFloat("Thirst.DefaultModifier", 1f);
        }
        
        public void OnTimeChanged(int gameHour) // Called when game hour changes
        {
            // int nightStartHour = _playerData.GetInt("Fatigue.NightStartHour", 22);
            // int nightEndHour = _playerData.GetInt("Fatigue.NightEndHour", 6);
            // var isNight = (gameHour >= nightStartHour || gameHour <= nightEndHour);

            // Fatigue.Modifier = isNight ? _playerData.GetFloat("Fatigue.NightModifier", 1.4f) : _playerData.GetFloat("Fatigue.DefaultModifier", 1f);
        }
        
        private void OnTimeSkipped(float hoursSkipped)
        {
            Debug.Log($"Time skipped: {hoursSkipped} hours. Updating player conditions...");
            var minutesSkipped = hoursSkipped * 60f;
    
            foreach (var stat in _allStats)
            {
                if (stat is Hunger || stat is Thirst || stat is Fatigue)
                {
                    // UpdateStat expects a delta, and BaseIncreaseRate is per minute.
                    stat.UpdateCondition(minutesSkipped);
                    Debug.Log($"Updated {stat.GetType().Name} after time skip. New value: {stat.Value}");
                }
                // Health and Stamina might need special handling for time skips if they have complex logic
                // For example, Stamina should likely be fully recovered. Health might regenerate.
                else if (stat is Stamina stamina)
                {
                    stamina.Increase(stamina.MaxValue); // Fully recover stamina on time skip
                     Debug.Log($"Stamina fully recovered after time skip. New value: {stamina.Value}");
                }
                else if (stat is Health health)
                {
                    // Optional: Passive health regeneration during time skip
                    // float healthRegenPerMinute = health.BaseIncreaseRate; // If BaseIncreaseRate is per minute
                    // health.Increase(healthRegenPerMinute * minutesSkipped * health.Modifier);
                    // Debug.Log($"Updated Health after time skip. New value: {health.Value}");
                }
            }
        }
    }
}