namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public class Fatique : StatBase
    {
        public Fatique(float max, float start)
            : base(0, max, start, StatCriticalDirection.High) { }
        
        public override float GetCriticalThreshold() => Max * 0.8f;
    }
}