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
        // public static event Action<float, float> OnHealthChanged;
        // public static event Action<float, float> OnStaminaChanged;
        // public static event Action<float, float> OnHungerChanged;
        // public static event Action<float, float> OnThirstChanged;
        // public static event Action<float, float> OnFatigueChanged;
        // public static event Action OnDie;
        #endregion
        
        public Health Health { get; private set; }
        public Stamina Stamina { get; private set; }
        public Hunger Hunger { get; private set; }
        public Thirst Thirst { get; private set; }
        public Fatigue Fatigue { get; private set; }

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
        private int _lastSecond = -1;

        private void Awake()
        {
            Health = new Health(100f, 1f);
            Stamina = new Stamina(100f, 10f, 20f);
            Hunger = new Hunger(100f, 1f);
            Thirst = new Thirst(100f, 1f);
            Fatigue = new Fatigue(100f, 1f);
            
            _allStats = new List<ICondition> { Health, Stamina, Hunger, Thirst, Fatigue };

            FrameBasedUpdates().Forget();
            // SecondBasedUpdates().Forget();
            MinuteBasedUpdates().Forget();
        }

#if false
        private async UniTaskVoid StartTimeBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                var currentMinute = GameTime.Instance.GameMinute;

                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    
                    foreach (var stat in _allStats)
                    {
                        if (stat != Health && stat != Stamina)
                            stat.UpdateStat(1.0f);
                    }

                    // OnHungerChanged?.Invoke(Hunger.Value, Hunger.MaxValue);
                    // OnThirstChanged?.Invoke(Thirst.Value, Thirst.MaxValue);
                    // OnFatigueChanged?.Invoke(Fatigue.Value, Fatigue.MaxValue);
                    //
                    // // Açlık/susuzluk tavan yaparsa sağlık azalsın
                    // if (Hunger.Value >= Hunger.MaxValue || Thirst.Value >= Thirst.MaxValue)
                    //     Health.Decrease(1f);
                    //
                    // if (Health.Value <= 0)
                    //     Die();
                }

                // await UniTask.Delay(100);
                await UniTask.Yield(PlayerLoopTiming.Update); // Bir sonraki frame’e kadar bekle
            }
        }
#endif

        #region Updates
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                // Her frame'de sadece Health ve Stamina güncellensin!
                var delta = Time.deltaTime;
                Health.UpdateStat(delta);
                Stamina.UpdateStat(delta);

                // Debug ve Event update için her frame güncelle
                health = Health.Value;
                stamina = Stamina.Value;
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        private async UniTaskVoid SecondBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                var currentSecond = GameTime.Instance.GameSecond;
                if (currentSecond != _lastSecond)
                {
                    _lastSecond = currentSecond;
                    foreach (var stat in _allStats)
                    {
                        if (stat != Health && stat != Stamina)
                            stat.UpdateStat(1.0f);
                    }
                }
                
                UpdateInspectorData();

                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private async UniTaskVoid MinuteBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    foreach (var stat in _allStats)
                    {
                        if (stat != Health && stat != Stamina)
                            stat.UpdateStat(1.0f);
                    }
                }
                
                UpdateInspectorData();
                
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        #endregion

        
        public void Eat(float amount) => Hunger.Decrease(amount);
        public void Drink(float amount) => Thirst.Decrease(amount);
        public void Sleep(float hours) => Fatigue.Decrease(hours * 20f);

        private void UpdateInspectorData()
        {
            hunger = Hunger.Value;
            thirst = Thirst.Value;
            fatigue = Fatigue.Value;
        }
        
        // private void Die()
        // {
        //     OnDie?.Invoke();
        // }

        #region Condition Modifiers
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
        #endregion
        
    }
}