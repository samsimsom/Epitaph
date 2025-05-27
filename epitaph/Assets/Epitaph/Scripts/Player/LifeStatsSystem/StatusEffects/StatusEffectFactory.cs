namespace Epitaph.Scripts.Player.LifeStatsSystem.StatusEffects
{
    public static class StatusEffectFactory
    {
        // Statik olarak isme göre status effect oluşturur. Buraya ekle!
        public static IStatusEffect Create(string name)
        {
            switch (name)
            {
                case "Poisoned": return new PoisonedEffect();
                // Buraya yeni effect ekle!
                default: return null;
            }
        }
    }
}