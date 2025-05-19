using System.Collections.Generic;
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

        [SerializeField] private Vector4 stats; 

        private void Awake()
        {
            Health = new HealthCondition(100f);
            Hunger = new HungerCondition(100f, 0.5f);
            Thirst = new ThirstCondition(100f, 0.8f);
            Fatigue = new FatigueCondition(100f, 0.3f);
            
            _allStats = new List<ICondition> { Health, Hunger, Thirst, Fatigue };
        }

        private void Update()
        {
            foreach (var stat in _allStats)
                stat.UpdateStat(Time.deltaTime);

            stats = new Vector4(Health.Value, Hunger.Value, Thirst.Value, Fatigue.Value);
            
            // Açlık/susuzluk tepeye çıkarsa sağlık azalır:
            if (Hunger.Value >= Hunger.MaxValue || Thirst.Value >= Thirst.MaxValue)
                Health.Decrease(10f * Time.deltaTime);

            if (Health.Value <= 0)
                Die();
        }

        public void Eat(float amount) => Hunger.Decrease(amount);
        public void Drink(float amount) => Thirst.Decrease(amount);
        public void Sleep(float hours) => Fatigue.Decrease(hours * 20f);

        private void Die()
        {
            Debug.Log("Player died!");
        }
    }
}