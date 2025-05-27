namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public class Stamina : StatBase
    {
        public Stamina(float max, float start) 
            : base(0, max, start, StatCriticalDirection.Low) { }
        
        public override float GetCriticalThreshold() => Max * 0.1f;
    }
}