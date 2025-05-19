using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem
{
    public class HealthCondition : ICondition
    {
        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float RegenRate { get; set; } = 0f; // Opsiyonel

        public HealthCondition(float max)
        {
            MaxValue = max;
            Value = max;
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
            // Eğer pasif iyileşme olacaksa buraya yazılır
            // Value = Mathf.Clamp(Value + RegenRate * deltaTime, 0, MaxValue);
        }
    }
}