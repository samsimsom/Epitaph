using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Fatigue : ICondition
    {
        public float Value { get; private set; } // Current fatigue level (0 = not fatigued, MaxValue = very fatigued)
        public float MaxValue { get; private set; } // The point at which "very fatigued" state begins
        public float BaseIncreaseRate { get; set; } // Rate at which fatigue increases
        public float BaseDecreaseRate { get; set; } // Rate at which fatigue decreases (e.g. sleeping) - used by external actions
        public float Modifier { get; set; } = 1f; // Affects BaseIncreaseRate
        
        private float _exhaustionThreshold; // Additional value beyond MaxValue for "exhaustion"
        public float CurrentExhaustionPoint => MaxValue + _exhaustionThreshold;

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        // EffectiveDecreaseRate can be used by external actions like Sleep if Modifier should apply there too

        public Fatigue(float initialValue, float maxValue, float baseIncreaseRate, float exhaustionThreshold, float baseDecreaseRate = 0f)
        {
            MaxValue = maxValue;
            _exhaustionThreshold = exhaustionThreshold;
            Value = Mathf.Clamp(initialValue, 0, CurrentExhaustionPoint);
            BaseIncreaseRate = baseIncreaseRate;
            BaseDecreaseRate = baseDecreaseRate; // Store this if sleeping effectiveness depends on it
        }

        public void Increase(float amount) // Make more fatigued
        {
            Value = Mathf.Clamp(Value + amount, 0, CurrentExhaustionPoint);
            CheckExhaustion();
        }

        public void Decrease(float amount) // Make less fatigued (e.g., sleeping)
        {
            Value = Mathf.Clamp(Value - amount, 0, CurrentExhaustionPoint);
        }

        public void UpdateCondition(float deltaTime)
        {
            Increase(EffectiveIncreaseRate * deltaTime);
        }

        public void CheckExhaustion()
        {
            if (Value >= CurrentExhaustionPoint)
            {
                Debug.Log("Player is Exhausted!");
                // Trigger events or apply debuffs
            }
            else if (Value >= MaxValue)
            {
                Debug.Log("Player is Very Fatigued!");
            }
        }
    }
}