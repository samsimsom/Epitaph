using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Thirst : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float BaseIncreaseRate { get; set; }
        public float BaseDecreaseRate { get; set; }
        public float Modifier { get; set; } = 1f;
        
        private float _dehydrationThreshold = 25.0f;

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        public float EffectiveDecreaseRate => BaseDecreaseRate * Modifier;

        public Thirst(float max, float rate)
        {
            MaxValue = max;
            Value = 0;
            BaseIncreaseRate = rate;
        }

        public void Increase(float amount)
        {
            Value = Mathf.Clamp(Value + amount, 0, MaxValue);
        }

        public void Decrease(float amount)
        {
            Value = Mathf.Clamp(Value - amount, 0, MaxValue);
        }

        public void UpdateStat(float deltaTime)
        {
            Value = Mathf.Clamp(Value + EffectiveIncreaseRate * deltaTime, 0,
                MaxValue + _dehydrationThreshold);
            CheckDehydration();
        }
        
        public void CheckDehydration()
        {
            if (Value >= MaxValue + _dehydrationThreshold)
            {
                Debug.Log("Start Dehydration!");
            }
        }
        
    }
}