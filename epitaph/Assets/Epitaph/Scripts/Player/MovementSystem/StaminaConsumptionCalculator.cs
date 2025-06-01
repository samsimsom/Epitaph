using Epitaph.Scripts.Player.LifeStatsSystem;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public static class StaminaConsumptionCalculator
    {
        // Base consumption rates
        private const float BaseRunConsumption = 15f;
        private const float BaseWalkConsumption = 0.0f;
        private const float BaseCrouchConsumption = 1f;
        private const float BaseJumpConsumption = 10f;
        
        private static float GetBaseConsumption(MovementBehaviour movement)
        {
            if (movement.IsRunning) return BaseRunConsumption;
            if (movement.IsWalking) return BaseWalkConsumption;
            if (movement.IsCrouching) return BaseCrouchConsumption;
            return 0f; // Idle
        }
        
        public static float MovementConsumption(MovementBehaviour movement, 
            LifeStatsManager lifeStats)
        {
            var baseConsumption = GetBaseConsumption(movement);
            
            if (baseConsumption <= 0f) return 0f; // No movement, no consumption
            
            // Environmental and health factors
            var temperatureMultiplier = TemperatureMultiplier(lifeStats);
            var healthMultiplier = HealthMultiplier(lifeStats);
            var fatigueMultiplier = FatigueMultiplier(lifeStats);
            
            return baseConsumption * temperatureMultiplier * healthMultiplier * fatigueMultiplier;
        }
        
        public static float JumpConsumption(LifeStatsManager lifeStats)
        {
            var baseConsumption = BaseJumpConsumption;
            
            // Fatigue affects jump stamina cost
            var fatigueMultiplier = FatigueMultiplier(lifeStats);
            var healthMultiplier = HealthMultiplier(lifeStats);
            
            return baseConsumption * fatigueMultiplier * healthMultiplier;
        }
        
        private static float TemperatureMultiplier(LifeStatsManager lifeStats)
        {
            if (lifeStats.Temperature.IsTooHigh) return 1.4f;
            if (lifeStats.Temperature.IsTooLow) return 1.2f;
            return 1f;
        }
        
        private static float HealthMultiplier(LifeStatsManager lifeStats)
        {
            var healthRatio = lifeStats.Health.Current / lifeStats.Health.Max;
            return Mathf.Lerp(1.8f, 1f, healthRatio); // Low health = more stamina consumption
        }
        
        private static float FatigueMultiplier(LifeStatsManager lifeStats)
        {
            if (lifeStats.Fatique.IsCritical) return 1.6f;
            
            var fatigueRatio = lifeStats.Fatique.Current / lifeStats.Fatique.Max;
            return Mathf.Lerp(1f, 1.4f, fatigueRatio); // More fatigue = more stamina consumption
        }
    }
}