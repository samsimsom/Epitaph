namespace Epitaph.Scripts.Player.HealthSystem
{
    public interface ICondition
    {
        float Value { get; }
        float MaxValue { get; }
        float BaseIncreaseRate { get; set; }
        public float Modifier { get; set; }
        void Increase(float amount);
        void Decrease(float amount);
        void UpdateStat(float deltaTime);
    }
}