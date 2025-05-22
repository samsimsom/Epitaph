namespace Epitaph.Scripts.Player.HealthSystem
{
    public interface ICondition
    {
        public float Value { get; }
        public float MaxValue { get; }
        public float BaseIncreaseRate { get; set; }
        public float BaseDecreaseRate { get; set; }
        public float Modifier { get; set; }
        public void Increase(float amount);
        public void Decrease(float amount);
        public void UpdateCondition(float deltaTime);
    }
}