namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public class Temperature : StatBase
    {
        public float MinSafe { get; }
        public float MaxSafe { get; }

        public Temperature(float min, float max, float minSafe, float maxSafe, float start)
            : base(min, max, start, StatCriticalDirection.Range)
        {
            MinSafe = minSafe;
            MaxSafe = maxSafe;
        }

        public override (float low, float high) GetRangeThreshold() => (MinSafe, MaxSafe);

        public override float GetCriticalThreshold() => 0f; // Not used
        public bool IsTooLow => Current < MinSafe;
        public bool IsTooHigh => Current > MaxSafe;
        public bool IsSafe => Current >= MinSafe && Current <= MaxSafe;
    }
}