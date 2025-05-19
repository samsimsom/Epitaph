using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class PlayerCondition : MonoBehaviour
    {
        #region Events
        public static event Action<float, float> OnHealthChanged;
        public static event Action<float, float> OnHungerChanged;
        public static event Action<float, float> OnThirstChanged;
        public static event Action<float, float> OnFatigueChanged;
        public static event Action OnDie;
        #endregion
        
        public HealthCondition Health { get; private set; }
        public StaminaCondition Stamina { get; private set; }
        public HungerCondition Hunger { get; private set; }
        public ThirstCondition Thirst { get; private set; }
        public FatigueCondition Fatigue { get; private set; }

        private List<ICondition> _allStats;

        #region Debug
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float health;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float stamina;
        [Space]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float hunger;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float thirst;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float fatigue;
        #endregion
        
        private int _lastMinute = -1;

        private void Awake()
        {
            Health = new HealthCondition(100f, 1f);
            Stamina = new StaminaCondition(100f, 10f, 20f);
            Hunger = new HungerCondition(100f, 0.25f);
            Thirst = new ThirstCondition(100f, 0.5f);
            Fatigue = new FatigueCondition(100f, 0.1f);
            
            _allStats = new List<ICondition> { Health, Stamina, Hunger, Thirst, Fatigue };
                
            StartTimeBasedUpdates().Forget();
        }
        
        private void Update()
        {
            // Her frame'de sadece Health ve Stamina güncellensin!
            var delta = Time.deltaTime;
            Health.UpdateStat(delta);
            Stamina.UpdateStat(delta);

            // Debug ve Event update için her frame güncelle
            health = Health.Value;
            stamina = Stamina.Value;
            OnHealthChanged?.Invoke(Health.Value, Health.MaxValue);

            // Gerekirse stamina eventleri de burada eklenebilir
        }

        private async UniTaskVoid StartTimeBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                var currentMinute = GameTime.Instance.GameMinute;

                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    
                    // Sadece Health ve Stamina hariç diğerlerini güncelle
                    foreach (var stat in _allStats)
                    {
                        if (stat != Health && stat != Stamina)
                            stat.UpdateStat(1.0f); // Dakika başı 1 birimlik ilerleme
                    }

                    // Debug
                    hunger = Hunger.Value;
                    thirst = Thirst.Value;
                    fatigue = Fatigue.Value;

                    OnHungerChanged?.Invoke(Hunger.Value, Hunger.MaxValue);
                    OnThirstChanged?.Invoke(Thirst.Value, Thirst.MaxValue);
                    OnFatigueChanged?.Invoke(Fatigue.Value, Fatigue.MaxValue);

                    // Açlık/susuzluk tavan yaparsa sağlık azalsın
                    if (Hunger.Value >= Hunger.MaxValue || Thirst.Value >= Thirst.MaxValue)
                        Health.Decrease(1f);

                    if (Health.Value <= 0)
                        Die();
                }

                await UniTask.Delay(100);
            }
        }

        public void Eat(float amount) => Hunger.Decrease(amount);
        public void Drink(float amount) => Thirst.Decrease(amount);
        public void Sleep(float hours) => Fatigue.Decrease(hours * 20f);

        private void Die()
        {
            OnDie?.Invoke();
        }
        
        public void SetRunning(bool isRunning)
        {
            // Koşarken açlık %20, susuzluk %60 daha hızlı artsın
            Hunger.Modifier = isRunning ? 1.2f : 1f;
            Thirst.Modifier = isRunning ? 1.6f : 1f;
        }

        public void SetOutsideTemperature(float temperature)
        {
            // 35 derecenin üstü susuzluk artışını artırır, örnek
            Thirst.Modifier = temperature > 35f ? 1.5f : 1f;
        }

        public void OnTimeChanged(int gameHour)
        {
            if (gameHour >= 22 || gameHour <= 6) // Gece yorgunluk daha hızlı artabilir
                Fatigue.Modifier = 1.4f;
            else
                Fatigue.Modifier = 1f;
        }
        
    }
}