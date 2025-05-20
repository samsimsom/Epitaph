using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class Fatigue : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float BaseIncreaseRate { get; set; }
        public float Modifier { get; set; } = 1f;

        public float EffectiveIncreaseRate => BaseIncreaseRate * Modifier;


        public Fatigue(float max, float rate)
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
            Value = Mathf.Clamp(Value + EffectiveIncreaseRate * deltaTime, 0, MaxValue);
        }
    }
}