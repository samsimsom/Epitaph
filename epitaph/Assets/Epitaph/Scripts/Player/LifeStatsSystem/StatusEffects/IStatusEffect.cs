using System.Collections.Generic;

namespace Epitaph.Scripts.Player.LifeStatsSystem.StatusEffects
{
    public interface IStatusEffect
    {
        string Name { get; }
        float Duration { get; }
        bool IsExpired { get; }
        
        void ApplyEffect(LifeStatsManager stats, float deltaTime);
        void OnExpire(LifeStatsManager stats);
        
        Dictionary<string, object> GetSaveData(); // For serialization
        
        void SetSaveData(Dictionary<string, object> data); // For deserialization
    }
}