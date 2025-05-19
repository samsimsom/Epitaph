using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class HungerCondition : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float IncreaseRate { get; set; }

        public HungerCondition(float max, float rate)
        {
            MaxValue = max;
            Value = 0;
            IncreaseRate = rate;
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
            Value = Mathf.Clamp(Value + IncreaseRate * deltaTime, 0, MaxValue);
        }
    }
    public class ThirstCondition : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float IncreaseRate { get; set; }

        public ThirstCondition(float max, float rate)
        {
            MaxValue = max;
            Value = 0;
            IncreaseRate = rate;
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
            Value = Mathf.Clamp(Value + IncreaseRate * deltaTime, 0, MaxValue);
        }
    }
    
    public class FatigueCondition : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float IncreaseRate { get; set; }

        public FatigueCondition(float max, float rate)
        {
            MaxValue = max;
            Value = 0;
            IncreaseRate = rate;
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
            Value = Mathf.Clamp(Value + IncreaseRate * deltaTime, 0, MaxValue);
        }
    }
}

