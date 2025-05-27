namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeEvents
{
    public delegate void StatChangedHandler(string statName, float newValue, float oldValue);
    public delegate void StatCriticalHandler(string statName, float value);
    public delegate void DeathHandler();

    public interface ILifeStatsEvents
    {
        event StatChangedHandler OnStatChanged;
        event StatCriticalHandler OnStatCritical;
        event DeathHandler OnDeath;
    }
}