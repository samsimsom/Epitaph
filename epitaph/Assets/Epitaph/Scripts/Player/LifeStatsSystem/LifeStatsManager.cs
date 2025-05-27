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

namespace Epitaph.Scripts.Player.LifeStatsSystem
{
    public class LifeStatsManager : PlayerBehaviour, ILifeStatsEvents
    {
        public Health Health { get; }
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

        // Damage rates, tweak as needed
        private readonly float _temperatureDamageRate = 0.5f;
        private readonly float _hungerDamageRate = 0.3f;
        private readonly float _thirstDamageRate = 0.5f;
        private readonly float _fatiqueDamageRate = 0.2f;
        
        private bool _isUpdating;
        private int _lastMinute = -1;

        // --- Stat başlatıcı ---
        public LifeStatsManager(PlayerController playerController,
            float healthMax, float staminaMax, float fatiqueMax, float thirstMax, 
            float hungerMax, float temperatureMin, float temperatureMax, float tempMinSafe, 
            float tempMaxSafe, float tempStart) : base(playerController)
        {
            Health = new Health(healthMax, healthMax);
            Stamina = new Stamina(staminaMax, staminaMax);
            Fatique = new Fatique(fatiqueMax, 0f);
            Thirst = new Thirst(thirstMax, 0f);
            Hunger = new Hunger(hungerMax, 0f);
            Temperature = new Temperature(temperatureMin, temperatureMax, tempMinSafe, tempMaxSafe, tempStart);

            _stats = new Dictionary<string, StatBase>
            {
                {"Health", Health},
                {"Stamina", Stamina},
                {"Fatique", Fatique},
                {"Thirst", Thirst},
                {"Hunger", Hunger},
                {"Temperature", Temperature}
            };
        }

        // --- Stat erişim ve eventli değişim ---
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

        // --- Status effect yönetimi ---
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

        // --- Oyun döngüsü update fonksiyonu ---
        public void Update(float deltaTime, float activityLevel)
        {
            // Statlar arası ilişkiler:
            if (!Temperature.IsSafe)
            {
                if (Temperature.IsTooLow)
                {
                    AddStat("Health", -_temperatureDamageRate * deltaTime);
                    AddStat("Fatique", 0.2f * deltaTime);
                    AddStat("Stamina", -0.3f * deltaTime);
                }
                else if (Temperature.IsTooHigh)
                {
                    AddStat("Thirst", 1.5f * deltaTime);
                    AddStat("Health", -_temperatureDamageRate * deltaTime);
                    AddStat("Stamina", -0.2f * deltaTime);
                }
            }

            AddStat("Hunger", 0.2f * deltaTime * (1 + activityLevel));
            AddStat("Thirst", 0.3f * deltaTime * (1 + activityLevel + (Temperature.IsTooHigh ? 1 : 0)));
            AddStat("Fatique", 0.1f * deltaTime * (1 + activityLevel + (Hunger.IsCritical ? 1 : 0) + (Thirst.IsCritical ? 1 : 0)));

            if (activityLevel > 0)
                AddStat("Stamina", -1f * deltaTime * activityLevel * (1 + (Fatique.IsCritical ? 1 : 0)));
            else
                AddStat("Stamina", 1f * deltaTime * (Fatique.IsCritical ? 0.2f : 0.5f));

            if (Fatique.IsCritical)
                AddStat("Health", -_fatiqueDamageRate * deltaTime);
            if (Hunger.IsCritical)
                AddStat("Health", -_hungerDamageRate * deltaTime);
            if (Thirst.IsCritical)
                AddStat("Health", -_thirstDamageRate * deltaTime);

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

        // --- Serialization (Kaydet/Yükle) ---
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
        
        // ---------------------------------------------------------------------------- //

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
            // FrameBasedUpdates().Forget();
            MinuteBasedUpdates().Forget();
        }
        
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (_isUpdating)
            {
                Update(1.0f, 0.5f); 
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
                    Update(1.0f, 1.0f); 
                }
                
                await GameTime.Instance.WaitForGameSecond();
            }
        }
    }
}