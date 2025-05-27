namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public class Thirst : StatBase
    {
        public Thirst(float max, float start) 
            : base(0, max, start, StatCriticalDirection.High) { }
        
        public override float GetCriticalThreshold() => Max * 0.8f;
    }
}