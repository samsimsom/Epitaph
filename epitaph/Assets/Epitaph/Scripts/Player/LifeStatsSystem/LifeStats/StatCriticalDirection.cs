namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public enum StatCriticalDirection
    {
        Low,    // <= threshold kritik (Health gibi)
        High,   // >= threshold kritik (Hunger gibi)
        Range,  // thresholdLow ve thresholdHigh arası güvenli, dışı kritik (Temperature gibi)
        Custom  // Stat kendi IsCritical’ini override eder
    }
}