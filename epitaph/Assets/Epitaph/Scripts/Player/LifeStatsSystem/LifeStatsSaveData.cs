using System.Collections.Generic;

namespace Epitaph.Scripts.Player.LifeStatsSystem
{
    public class LifeStatsSaveData
    {
        public Dictionary<string, float> StatValues { get; set; }
        public List<StatusEffectSaveData> StatusEffects { get; set; }
    }
}