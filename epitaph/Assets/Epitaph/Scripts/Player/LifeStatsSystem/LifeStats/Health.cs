namespace Epitaph.Scripts.Player.LifeStatsSystem.LifeStats
{
    public class Health : StatBase
    {
        public Health(float max, float start) 
            : base(0, max, start, StatCriticalDirection.Low) { }
        
        public override float GetCriticalThreshold() => Max * 0.1f;
        public bool IsDead => Current <= 0;
    }
}