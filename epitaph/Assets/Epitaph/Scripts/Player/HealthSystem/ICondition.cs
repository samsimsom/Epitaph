namespace Epitaph.Scripts.Player.HealthSystem
{
    public interface ICondition
    {
        public float Value { get; }
        public float MaxValue { get; }
        public float BaseIncreaseRate { get; }
        public float BaseDecreaseRate { get; }
        public float Modifier { get; }
        public void Increase(float amount);
        public void Decrease(float amount);
        public void UpdateStat(float deltaTime);
    }
}