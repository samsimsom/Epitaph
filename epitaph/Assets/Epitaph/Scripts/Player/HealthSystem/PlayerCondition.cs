using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class PlayerCondition : MonoBehaviour
    {
        public HealthCondition Health { get; private set; }
        public HungerCondition Hunger { get; private set; }
        public ThirstCondition Thirst { get; private set; }
        public FatigueCondition Fatigue { get; private set; }

        private List<ICondition> _allStats;
        
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float health;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float hunger;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float thirst;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private float fatigue;
        
        private int _lastMinute = -1;

        private void Awake()
        {
            Health = new HealthCondition(100f);
            Hunger = new HungerCondition(100f, 0.5f);
            Thirst = new ThirstCondition(100f, 0.8f);
            Fatigue = new FatigueCondition(100f, 0.3f);
            
            _allStats = new List<ICondition> { Health, Hunger, Thirst, Fatigue };
                
            StartTimeBasedUpdates().Forget();
        }

        private async UniTaskVoid StartTimeBasedUpdates()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                int currentMinute = GameTime.Instance.GameMinute;
                
                // Check if a minute has passed in game time
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    await UpdateStatsAsync();
                }
                
                // Wait a short time before next check to be efficient
                await UniTask.Delay(100);
            }
        }
        
        private async UniTask UpdateStatsAsync()
        {
            // Use a fixed time delta for per-minute updates
            var timeDelta = 1.0f;
            
            foreach (var stat in _allStats)
                stat.UpdateStat(timeDelta);
            
            health = Health.Value;
            hunger = Hunger.Value;
            thirst = Thirst.Value;
            fatigue = Fatigue.Value;
            
            // Açlık/susuzluk tepeye çıkarsa sağlık azalır:
            if (Hunger.Value >= Hunger.MaxValue || Thirst.Value >= Thirst.MaxValue)
                Health.Decrease(10f * timeDelta);

            if (Health.Value <= 0)
                Die();
                
            await UniTask.CompletedTask;
        }

        public void Eat(float amount) => Hunger.Decrease(amount);
        public void Drink(float amount) => Thirst.Decrease(amount);
        public void Sleep(float hours) => Fatigue.Decrease(hours * 20f);

        private void Die()
        {
            Debug.Log("Player died!");
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