using System;
using System.Collections.Generic;
using System.Linq;
using Epitaph.Scripts.Player.BaseBehaviour;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player.LifeStatsSystem.LifeEvents;
using Epitaph.Scripts.Player.LifeStatsSystem.LifeStats;
using Epitaph.Scripts.Player.LifeStatsSystem.StatusEffects;
using UnityEngine;

namespace Epitaph.Scripts.Player.LifeStatsSystem
{
    public class LifeStatsManager : PlayerBehaviour, ILifeStatsEvents
    {
        public Health Health { get; }
        public Vitality Vitality { get; }
        
        public Stamina Stamina { get; }
        public Fatique Fatique { get; }
        public Thirst Thirst { get; }
        public Hunger Hunger { get; }
        
        public Temperature Temperature { get; }

        public event StatChangedHandler OnStatChanged;
        public event StatCriticalHandler OnStatCritical;
        public event DeathHandler OnDeath;

        private readonly Dictionary<string, StatBase> _stats;
        private bool _isDead;
        private readonly List<IStatusEffect> _statusEffects = new();

        // 1 oyun dakikası = 2.5 saniye
        // private float _hungerPerGameMinute =  4.16f / 60f;    // +0.069
        // private float _thirstPerGameMinute =  4.16f / 60f;
        // private float _fatiquePerGameMinute = 6.25f / 60f;    // +0.104
        // private float _fatiqueSleepPerMinute = -12.5f / 60f;  // -0.208 (uykuda)
        
        // Damage rates, tweak as needed
        private readonly float _temperatureDamageRate = 0.5f;
        private readonly float _hungerDamageRate = 0.3f;
        private readonly float _thirstDamageRate = 0.5f;
        private readonly float _fatiqueDamageRate = 0.2f;
        
        // private bool _isSleeping = false;
        private bool _isUpdating;
        private int _lastMinute = -1;

        // --- Stat başlatıcı ---
        public LifeStatsManager(PlayerController playerController,
            float healthMax, float vitalityMax, float staminaMax, float fatiqueMax, float thirstMax, 
            float hungerMax, float temperatureMin, float temperatureMax, float tempMinSafe, 
            float tempMaxSafe, float tempStart) : base(playerController)
        {
            Health = new Health(healthMax, healthMax);
            Vitality = new Vitality(vitalityMax, vitalityMax);
            Stamina = new Stamina(staminaMax, staminaMax);
            
            Fatique = new Fatique(fatiqueMax, 0f);
            Thirst = new Thirst(thirstMax, 0f);
            Hunger = new Hunger(hungerMax, 0f);
            
            Temperature = new Temperature(temperatureMin, temperatureMax, tempMinSafe, 
                tempMaxSafe, tempStart);

            _stats = new Dictionary<string, StatBase>
            {
                {"Health", Health},
                {"Vitality", Vitality},
                {"Stamina", Stamina},
                {"Fatique", Fatique},
                {"Thirst", Thirst},
                {"Hunger", Hunger},
                {"Temperature", Temperature}
            };
        }
        

        #region Stat erişim ve eventli değişim

        public float GetStatValue(string statName)
        {
            return _stats.ContainsKey(statName) ? _stats[statName].Current : 0;
        }

        public void SetStat(string statName, float newValue)
        {
            if (!_stats.ContainsKey(statName)) return;
            var stat = _stats[statName];
            var oldValue = stat.Current;
            stat.Set(newValue);

            if (Math.Abs(stat.Current - oldValue) > float.Epsilon)
                OnStatChanged?.Invoke(statName, stat.Current, oldValue);

            if (stat.IsCritical)
                OnStatCritical?.Invoke(statName, stat.Current);

            if (statName == "Health" && Health.IsDead && !_isDead)
            {
                _isDead = true;
                OnDeath?.Invoke();
            }
        }

        public void AddStat(string statName, float amount)
        {
            if (!_stats.ContainsKey(statName)) return;
            SetStat(statName, _stats[statName].Current + amount);
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //

        #region Status Effect

        public void AddStatusEffect(IStatusEffect effect)
        {
            _statusEffects.Add(effect);
        }

        public bool HasStatusEffect(string effectName) =>
            _statusEffects.Any(se => se.Name == effectName);

        public void RemoveStatusEffect(string effectName)
        {
            _statusEffects.RemoveAll(se => se.Name == effectName);
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //

        #region Update Methods

        public void Update(float deltaTime, float activityLevel)
        {
            // Statlar arası ilişkiler:
            // AddStat("Hunger", _hungerPerGameMinute * deltaTime * (1 + activityLevel));
            // AddStat("Thirst", _thirstPerGameMinute * deltaTime * (1 + activityLevel + (Temperature.IsTooHigh ? 2f : 0)));
            
            UpdateStatsByActivity(deltaTime, activityLevel);

            if (Fatique.IsCritical)
                AddStat("Health", -_fatiqueDamageRate * deltaTime);
            
            if (Hunger.IsCritical)
                AddStat("Health", -_hungerDamageRate * deltaTime);
            
            if (Thirst.IsCritical)
                AddStat("Health", -_thirstDamageRate * deltaTime);
        }

        public void UpdateStatsByActivity(float deltaTime, float activityLevel)
        {
            AddStat("Hunger", 0.2f * deltaTime * (1 + activityLevel));
            AddStat("Thirst", 0.3f * deltaTime * (1 + activityLevel + (Temperature.IsTooHigh ? 1 : 0)));
            AddStat("Fatique", -0.1f * deltaTime * (1 + activityLevel + (Hunger.IsCritical ? 1 : 0) + (Thirst.IsCritical ? 1 : 0)));
            
            if (activityLevel > 0)
                AddStat("Stamina", -1f * deltaTime * activityLevel * (1 + (Fatique.IsCritical ? 1 : 0)));
            else
                AddStat("Stamina", 1f * deltaTime * (Fatique.IsCritical ? 0.2f : 0.5f));
            
            // if (_isSleeping)
            //     AddStat("Fatique", _fatiqueSleepPerMinute * deltaTime);
            // else
            //     AddStat("Fatique", _fatiquePerGameMinute * deltaTime * (1 + activityLevel));
        }
        
        public void UpdateStatsByTemperature(float deltaTime)
        {
            if (!Temperature.IsSafe)
            {
                if (Temperature.IsTooLow)
                {
                    AddStat("Health", -_temperatureDamageRate * deltaTime);
                    AddStat("Fatique", 0.2f * deltaTime);
                    AddStat("Vitality", -0.3f * deltaTime);
                }
                else if (Temperature.IsTooHigh)
                {
                    AddStat("Thirst", 1.5f * deltaTime);
                    AddStat("Health", -_temperatureDamageRate * deltaTime);
                    AddStat("Vitality", -0.2f * deltaTime);
                }
            }
        }

        public void UpdateStatsByVitality(float deltaTime)
        {
            // 0-100 kotu | iyi
            var healthRatio = Health.Max / Mathf.Clamp(Health.Current, 1, Health.Max) / 10.0f; // 100 / 25 / 10 = 0.4; 
            
            // 100-0 kotu | iyi
            var hungerRatio = Mathf.Clamp(Hunger.Current, 1, Hunger.Max) / Hunger.Max; // 90 / 100 = 0.9
            var fatiqueRatio = Mathf.Clamp(Fatique.Current, 1, Fatique.Max) / Fatique.Max;
            var thirstRatio = Mathf.Clamp(Thirst.Current, 1, Thirst.Max) / Thirst.Max;
            
            var vitalityRatio = (healthRatio + hungerRatio + fatiqueRatio + thirstRatio) / 4.0f;

            // Güncelleme sıklığına (deltaTime) ve güncelleme hızınıza göre scale'i ayarlayın
            var vitalityChangeRate = 10f;

            if (vitalityRatio < 0.1f)
            {
                // İyileşiyor
                var increaseAmount = (0.5f - vitalityRatio) * vitalityChangeRate * deltaTime;
                AddStat("Vitality", increaseAmount);
            }
            else if (vitalityRatio > 0.15f && vitalityRatio < 0.3f)
            {
                // Kötüleşiyor
                var decreaseAmount = vitalityRatio * vitalityChangeRate * deltaTime;
                AddStat("Vitality", -decreaseAmount);
            }
        }

        public void UpdateStatsByStatusEffects(float deltaTime)
        {
            // Status effectler
            for (var i = _statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _statusEffects[i];
                effect.ApplyEffect(this, deltaTime);
                if (effect.IsExpired)
                {
                    effect.OnExpire(this);
                    _statusEffects.RemoveAt(i);
                }
            }
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
        #region Serialization (Kaydet/Yükle)

        public string SaveToJson()
        {
            var saveData = new LifeStatsSaveData
            {
                StatValues = _stats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Current),
                StatusEffects = _statusEffects.Select(se => new StatusEffectSaveData
                {
                    Name = se.Name,
                    Data = se.GetSaveData()
                }).ToList()
            };
            return JsonConvert.SerializeObject(saveData);
        }

        public void LoadFromJson(string json)
        {
            var saveData = JsonConvert.DeserializeObject<LifeStatsSaveData>(json);
            if (saveData == null) return;

            foreach (var kvp in saveData.StatValues)
            {
                if (_stats.ContainsKey(kvp.Key))
                    _stats[kvp.Key].Set(kvp.Value);
            }

            _statusEffects.Clear();
            foreach (var eff in saveData.StatusEffects)
            {
                var effect = StatusEffectFactory.Create(eff.Name);
                if (effect != null)
                {
                    effect.SetSaveData(eff.Data);
                    _statusEffects.Add(effect);
                }
            }
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //

        #region Monobehavior Methods

        public override void OnEnable()
        {
            _isUpdating = true;
        }

        public override void OnDisable()
        {
            _isUpdating = false; 
        }
        
        public override void Start()
        {
            FrameBasedUpdates().Forget();
            MinuteBasedUpdates().Forget();
        }
        
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (_isUpdating)
            {
                Update(0.01f, 0.0f);
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
                    UpdateStatsByTemperature(1.0f);
                    UpdateStatsByVitality(1.0f);
                    UpdateStatsByStatusEffects(1.0f);
                }
                
                await GameTime.Instance.WaitForGameMinutes();
            }
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}