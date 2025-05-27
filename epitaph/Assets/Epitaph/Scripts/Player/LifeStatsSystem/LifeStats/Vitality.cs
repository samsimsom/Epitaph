namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public class Vitality : StatBase
    {
        public Vitality(float max, float start) 
            : base(0, max, start, StatCriticalDirection.Low) { }
        
        public override float GetCriticalThreshold() => Max * 0.5f;
    }
}