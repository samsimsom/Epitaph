using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Thirst : ICondition
    {
        public float Value { get; private set; } // Current thirst level (0 = not thirsty, MaxValue = very thirsty)
        public float MaxValue { get; private set; } // The point at which "very thirsty" state begins
        public float BaseIncreaseRate { get; set; } // Rate at which thirst increases per unit of time
        public float BaseDecreaseRate { get; set; }
        public float Modifier { get; set; } = 1f;
        
        private float _dehydrationThreshold; // Additional value beyond MaxValue to reach "dehydration"
        public float CurrentDehydrationPoint => MaxValue + _dehydrationThreshold;

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;

        public Thirst(float initialValue, float maxValue, float baseIncreaseRate, float dehydrationThreshold, float baseDecreaseRate = 0f)
        {
            MaxValue = maxValue;
            _dehydrationThreshold = dehydrationThreshold;
            Value = Mathf.Clamp(initialValue, 0, CurrentDehydrationPoint);
            BaseIncreaseRate = baseIncreaseRate;
            BaseDecreaseRate = baseDecreaseRate;
        }

        public void Increase(float amount) // Make more thirsty
        {
            Value = Mathf.Clamp(Value + amount, 0, CurrentDehydrationPoint);
            CheckDehydration();
        }

        public void Decrease(float amount) // Make less thirsty (e.g., drinking)
        {
            Value = Mathf.Clamp(Value - amount, 0, CurrentDehydrationPoint);
        }

        public void UpdateCondition(float deltaTime)
        {
            Increase(EffectiveIncreaseRate * deltaTime);
        }
        
        public void CheckDehydration()
        {
            if (Value >= CurrentDehydrationPoint)
            {
                Debug.Log("Player is Dehydrated!");
                // Trigger events or apply debuffs
            }
            else if (Value >= MaxValue)
            {
                 Debug.Log("Player is Very Thirsty!");
            }
        }
    }
}