using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Hunger : ICondition
    {
        public float Value { get; private set; } // Current hunger level (0 = not hungry, MaxValue = very hungry)
        public float MaxValue { get; private set; } // The point at which "very hungry" state begins
        public float BaseIncreaseRate { get; set; } // Rate at which hunger increases per unit of time (e.g., per minute)
        public float BaseDecreaseRate { get; set; } // Not directly used by UpdateStat, potentially for other mechanics
        public float Modifier { get; set; } = 1f;   // Multiplies BaseIncreaseRate

        private float _starvingThreshold; // Additional value beyond MaxValue to reach "starving"
        public float CurrentStarvingPoint => MaxValue + _starvingThreshold; // Read-only property for clarity

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        // public float EffectiveDecreaseRate => BaseDecreaseRate * Modifier; // If BaseDecreaseRate were used

        // Constructor updated to take more configuration
        public Hunger(float initialValue, float maxValue, float baseIncreaseRate, float starvingThreshold, float baseDecreaseRate = 0f)
        {
            MaxValue = maxValue;
            _starvingThreshold = starvingThreshold;
            Value = Mathf.Clamp(initialValue, 0, CurrentStarvingPoint); // Initialize within full range
            BaseIncreaseRate = baseIncreaseRate;
            BaseDecreaseRate = baseDecreaseRate;
        }

        public void Increase(float amount) // Make more hungry
        {
            Value = Mathf.Clamp(Value + amount, 0, CurrentStarvingPoint);
            CheckStarving(); // Check after any increase
        }

        public void Decrease(float amount) // Make less hungry (e.g., eating)
        {
            Value = Mathf.Clamp(Value - amount, 0, CurrentStarvingPoint);
        }

        public void UpdateStat(float deltaTime) // deltaTime here is expected to be scaled appropriately (e.g., 1.0 for 1 minute)
        {
            Increase(EffectiveIncreaseRate * deltaTime); // Value increases over time
            // CheckStarving is called within Increase now
        }
        
        public void CheckStarving()
        {
            if (Value >= CurrentStarvingPoint)
            {
                Debug.Log("Player is Starving!");
                // Trigger events or apply debuffs here
            }
            else if (Value >= MaxValue)
            {
                Debug.Log("Player is Very Hungry!");
                 // Trigger events or apply milder debuffs here
            }
        }
    }
}