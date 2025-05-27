using System;
using System.Collections.Generic;

namespace Epitaph.Scripts.Player.LifeStatsSystem.StatusEffects
{
    public class PoisonedEffect : IStatusEffect
    {
        public string Name => "Poisoned";
        public float Duration { get; private set; }
        public bool IsExpired => Duration <= 0f;

        public PoisonedEffect(float duration = 30f)
        {
            Duration = duration;
        }

        public void ApplyEffect(LifeStatsManager stats, float deltaTime)
        {
            stats.AddStat("Health", -0.5f * deltaTime);
            Duration -= deltaTime;
        }

        public void OnExpire(LifeStatsManager stats)
        {
            // Optional: effect expires
        }

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object> { { "Duration", Duration } };
        }

        public void SetSaveData(Dictionary<string, object> data)
        {
            if (data.TryGetValue("Duration", out var val))
                Duration = Convert.ToSingle(val);
        }
    }
}