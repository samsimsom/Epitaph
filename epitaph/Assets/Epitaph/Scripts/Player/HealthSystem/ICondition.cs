namespace Epitaph.Scripts.Player.HealthSystem
{
    public interface ICondition
    {
        float Value { get; }
        float MaxValue { get; }
        void Increase(float amount);
        void Decrease(float amount);
        void UpdateStat(float deltaTime);
    }
}