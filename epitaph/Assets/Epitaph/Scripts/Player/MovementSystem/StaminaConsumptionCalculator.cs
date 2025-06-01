using Epitaph.Scripts.Player.LifeStatsSystem;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public static class StaminaConsumptionCalculator
    {
        // Base consumption rates
        private const float BaseRunConsumption = 15f;
        private const float BaseWalkConsumption = 0.1f;
        private const float BaseCrouchConsumption = 1f;
        private const float BaseJumpConsumption = 10f;
        
        public static float CalculateMovementConsumption(MovementBehaviour movement, 
            LifeStatsManager lifeStats)
        {
            var baseConsumption = GetBaseConsumption(movement);
            
            if (baseConsumption <= 0f) return 0f; // No movement, no consumption
            
            // Environmental and health factors
            var temperatureMultiplier = CalculateTemperatureMultiplier(lifeStats);
            var healthMultiplier = CalculateHealthMultiplier(lifeStats);
            var fatigueMultiplier = CalculateFatigueMultiplier(lifeStats);
            
            return baseConsumption * temperatureMultiplier * healthMultiplier * fatigueMultiplier;
        }
        
        public static float CalculateJumpConsumption(LifeStatsManager lifeStats)
        {
            var baseConsumption = BaseJumpConsumption;
            
            // Fatigue affects jump stamina cost
            var fatigueMultiplier = CalculateFatigueMultiplier(lifeStats);
            var healthMultiplier = CalculateHealthMultiplier(lifeStats);
            
            return baseConsumption * fatigueMultiplier * healthMultiplier;
        }
        
        private static float GetBaseConsumption(MovementBehaviour movement)
        {
            if (movement.IsRunning) return BaseRunConsumption;
            if (movement.IsWalking) return BaseWalkConsumption;
            if (movement.IsCrouching) return BaseCrouchConsumption;
            return 0f; // Idle
        }
        
        private static float CalculateTemperatureMultiplier(LifeStatsManager lifeStats)
        {
            if (lifeStats.Temperature.IsTooHigh) return 1.4f;
            if (lifeStats.Temperature.IsTooLow) return 1.2f;
            return 1f;
        }
        
        private static float CalculateHealthMultiplier(LifeStatsManager lifeStats)
        {
            var healthRatio = lifeStats.Health.Current / lifeStats.Health.Max;
            return Mathf.Lerp(1.8f, 1f, healthRatio); // Low health = more stamina consumption
        }
        
        private static float CalculateFatigueMultiplier(LifeStatsManager lifeStats)
        {
            if (lifeStats.Fatique.IsCritical) return 1.6f;
            
            var fatigueRatio = lifeStats.Fatique.Current / lifeStats.Fatique.Max;
            return Mathf.Lerp(1f, 1.4f, fatigueRatio); // More fatigue = more stamina consumption
        }
    }
}