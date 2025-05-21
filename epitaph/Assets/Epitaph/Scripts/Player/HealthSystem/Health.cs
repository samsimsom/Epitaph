using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Health : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float BaseIncreaseRate { get; set; } // Passive regeneration rate
        public float BaseDecreaseRate { get; set; } // Not used in this class's UpdateStat, but can be for external damage logic
        public float Modifier { get; set; } = 1f; // General modifier, could affect regeneration or damage intake

        // Effective rates can be used if passive regeneration/degeneration is implemented in UpdateStat
        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;
        public float EffectiveDecreaseRate => BaseDecreaseRate * Modifier;

        public Health(float initialValue, float maxValue, float baseIncreaseRate, float baseDecreaseRate = 0f)
        {
            MaxValue = maxValue;
            Value = Mathf.Clamp(initialValue, 0, MaxValue);
            BaseIncreaseRate = baseIncreaseRate;
            BaseDecreaseRate = baseDecreaseRate;
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
            // Example: Passive regeneration
            // if (BaseIncreaseRate > 0)
            // {
            //     Increase(EffectiveIncreaseRate * deltaTime);
            // }
        }
    }
}