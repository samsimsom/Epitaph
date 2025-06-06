using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        
        public float VitalityRatio { get; private set; }

        public event StatChangedHandler OnStatChanged;
        public event StatCriticalHandler OnStatCritical;
        public event DeathHandler OnDeath;

        private readonly Dictionary<string, StatBase> _stats;
        private bool _isDead;
        private readonly List<IStatusEffect> _statusEffects = new();

        // 1 oyun dakikası = 2.5 saniye
        private float _hungerPerGameMinute =  4.16f / 60f;    // +0.069
        private float _thirstPerGameMinute =  4.16f / 60f;
        private float _fatiquePerGameMinute = 6.25f / 60f;    // +0.104
        // private float _fatiqueSleepPerMinute = -12.5f / 60f;  // -0.208 (uykuda)
        
        // Damage rates, tweak as needed
        private readonly float _temperatureDamageRate = 0.5f;
        private readonly float _hungerDamageRate = 0.3f;
        private readonly float _thirstDamageRate = 0.5f;
        private readonly float _fatiqueDamageRate = 0.2f;
        
        // private bool _isSleeping = false;
        private bool _isUpdating;
        private int _lastMinute = -1;
        
        private CancellationTokenSource _frameBasedUpdateCts;
        private CancellationTokenSource _minuteBasedUpdateCts;

        
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
        

        #region Stat Methods

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

        public void DecreaseHealth(float deltaTime)
        {
            if (Fatique.IsCritical)
                AddStat("Health", -_fatiqueDamageRate * deltaTime);
            
            if (Hunger.IsCritical)
                AddStat("Health", -_hungerDamageRate * deltaTime);
            
            if (Thirst.IsCritical)
                AddStat("Health", -_thirstDamageRate * deltaTime);
        }
        public void IncreaseHealth(float deltaTime){ }

        public void DecreaseThirst(float deltaTime, float activityLevel)
        {
            AddStat("Thirst", _thirstPerGameMinute * deltaTime * 
                              (1 + activityLevel + (Temperature.IsTooHigh ? 1 : 0)));
        }
        public void IncreaseThirst(float deltaTime, float activityLevel)
        {
            AddStat("Thirst", -_thirstPerGameMinute * deltaTime * 
                              (1 + activityLevel + (Temperature.IsTooHigh ? 1 : 0)));
        }

        public void DecreaseFatique(float deltaTime, float activityLevel)
        {
            AddStat("Fatique", _fatiquePerGameMinute * deltaTime * 
                               (1 + activityLevel + (Hunger.IsCritical ? 1 : 0) + (Thirst.IsCritical ? 1 : 0)));
        }
        public void IncreaseFatique(float deltaTime, float activityLevel)
        {
            AddStat("Fatique", -_fatiquePerGameMinute * deltaTime * 
                               (1 + activityLevel + (Hunger.IsCritical ? 1 : 0) + (Thirst.IsCritical ? 1 : 0)));
        }
        
        public void DecreaseHunger(float deltaTime, float activityLevel)
        {
            AddStat("Hunger", _hungerPerGameMinute * deltaTime * (1 + activityLevel));
        }
        public void IncreaseHunger(float deltaTime, float activityLevel)
        {
            AddStat("Hunger", -_hungerPerGameMinute * deltaTime * (1 + activityLevel));
        }
        
        public void DecreaseStamina(float deltaTime, float activityLevel)
        {
            AddStat("Stamina", -1f * deltaTime * activityLevel * (1 + (Fatique.IsCritical ? 1 : 0)));
        }
        public void IncreaseStamina(float deltaTime)
        {
            AddStat("Stamina", 20f * deltaTime * (Fatique.IsCritical ? 0.2f : 0.5f));
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
                    AddStat("Health", -_temperatureDamageRate * deltaTime);
                    
                    AddStat("Thirst", 1.5f * deltaTime);
                    AddStat("Vitality", -0.2f * deltaTime);
                }
            }
        }
        
        public void UpdateVitality(float deltaTime)
        {
            // Health için normalleştirilmiş oran (1'e yaklaştıkça daha iyi)
            var healthNormalized = Mathf.Clamp01(Health.Current / Health.Max);
    
            // Diğer parametreler için normalleştirilmiş oranlar (0'a yaklaştıkça daha iyi)
            var hungerNormalized = 1f - Mathf.Clamp01(Hunger.Current / Hunger.Max);
            var fatiqueNormalized = 1f - Mathf.Clamp01(Fatique.Current / Fatique.Max);
            var thirstNormalized = 1f - Mathf.Clamp01(Thirst.Current / Thirst.Max);
    
            // Tüm değerlerin ağırlıklı ortalaması (0-1 aralığında)
            // Health'e daha fazla ağırlık verebilirsiniz
            var healthWeight = 1.5f; // Health için ağırlık
            var otherWeight = 1.0f;  // Diğer faktörler için ağırlık
    
            var totalWeight = healthWeight + (otherWeight * 3); // Toplam ağırlık
    
            // Ağırlıklı ortalama (0-1 aralığında)
            VitalityRatio = (healthNormalized * healthWeight + 
                             hungerNormalized * otherWeight + 
                             fatiqueNormalized * otherWeight + 
                             thirstNormalized * otherWeight) / totalWeight;
    
            // Vitality stat'ını güncelle
            var vitalityChangeRate = 10f;
    
            // Mevcut Vitality ile hesaplanan VitalityRatio arasındaki farkı azalt
            var currentVitalityNormalized = Vitality.Current / Vitality.Max;
            var diff = VitalityRatio - currentVitalityNormalized;
    
            // Değişim miktarı (pozitif veya negatif olabilir)
            var changeAmount = Mathf.Abs(diff) * vitalityChangeRate * deltaTime;
    
            // İstenen aralıklara göre Vitality değerini güncelle
            if (VitalityRatio >= 0.9f && VitalityRatio <= 1.0f)
            {
                // 0.9-1.0 aralığında pozitif değişim
                AddStat("Vitality", changeAmount);
            }
            else if (VitalityRatio >= 0.5f && VitalityRatio < 0.9f)
            {
                // 0.5-0.9 aralığında negatif değişim
                AddStat("Vitality", -changeAmount);
            }
            
            // Debug.Log($"VitalityRatio: {VitalityRatio:F2} | Health: {healthNormalized:F2} | " +
            //           $"Hunger: {hungerNormalized:F2} | Fatique: {fatiqueNormalized:F2} | " +
            //           $"Thirst: {thirstNormalized:F2}");
            // Debug.Log($"Vitality Change: {changeAmount:F2} | Vitality: {Vitality.Current:F2}");
        }
        
        public void UpdateStatusEffects(float deltaTime)
        {
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
        
        public override void Start()
        {
            StartFrameBasedUpdates();
            StartMinuteBasedUpdates();
        }
        
        #endregion

        // ---------------------------------------------------------------------------- //

        #region Frame Based Update

        public void StartFrameBasedUpdates()
        {
            StopFrameBasedUpdates();
            _frameBasedUpdateCts = new CancellationTokenSource();
            FrameBasedUpdates(_frameBasedUpdateCts.Token).Forget();
        }

        public void StopFrameBasedUpdates()
        {
            _frameBasedUpdateCts?.Cancel();
            _frameBasedUpdateCts = null;
        }
        
        private async UniTaskVoid FrameBasedUpdates(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Stamina.SetMax(Vitality.Current);
                DecreaseHealth(0.01f);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //

        #region Minute Based Update

        public void StartMinuteBasedUpdates()
        {
            StopMinuteBasedUpdates();
            _minuteBasedUpdateCts = new CancellationTokenSource();
            MinuteBasedUpdates(_minuteBasedUpdateCts.Token).Forget();
        }

        public void StopMinuteBasedUpdates()
        {
            _minuteBasedUpdateCts?.Cancel();
            _minuteBasedUpdateCts = null;
        }
        
        private async UniTaskVoid MinuteBasedUpdates(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (GameTime.Instance == null)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token); 
                    continue;
                }

                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    DecreaseThirst(1.0f, 1.0f);
                    DecreaseHunger(1.0f, 1.0f);
                    DecreaseFatique(1.0f, 1.0f);
                    UpdateStatsByTemperature(1.0f);
                    UpdateVitality(1.0f);
                    UpdateStatusEffects(1.0f);
                }
                
                await GameTime.Instance.WaitForGameMinutes(token);
            }
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}