using System;

namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public abstract class StatBase
    {
        public float Current { get; protected set; }
        public float Min { get; protected set; }
        public float Max { get; protected set; }
        public StatCriticalDirection CriticalDirection { get; protected set; }

        public StatBase(float min, float max, float start, StatCriticalDirection direction)
        {
            Min = min;
            Max = max;
            Current = Clamp(start);
            CriticalDirection = direction;
        }

        public virtual void Add(float amount) { Set(Current + amount); }
        public virtual void Set(float value) { Current = Clamp(value); }
        protected float Clamp(float value) => MathF.Max(Min, MathF.Min(Max, value));

        public virtual float GetCriticalThreshold() => 0f;
        public virtual (float low, float high) GetRangeThreshold() => (Min, Max);

        public virtual bool IsCritical
        {
            get
            {
                switch (CriticalDirection)
                {
                    case StatCriticalDirection.Low:
                        return Current <= GetCriticalThreshold();
                    case StatCriticalDirection.High:
                        return Current >= GetCriticalThreshold();
                    case StatCriticalDirection.Range:
                        var (low, high) = GetRangeThreshold();
                        return Current < low || Current > high;
                    default:
                        return false;
                }
            }
        }
        
        // public virtual void Increase() { }
        // public virtual void Decrease() { }
        // public virtual void Restore() { }
        // public virtual void Drain() { }
    }

}