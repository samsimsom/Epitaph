using UnityEngine;

namespace Epitaph.Scripts.Player.VitalSystem
{
    public class StaminaVital : VitalBase
    {
        public override float Value { get; set; } = 0f;
        public override float MinValue { get; set; } = 0f;
        public override float MaxValue { get; set; } = 100f;
        public override float BaseIncreaseRate { get; set; } = 1.0f;
        public override float BaseDecreaseRate { get; set; } = 1.0f;
        public override float Modifier { get; set; } = 1.0f;
        
        public StaminaVital(VitalBehaviour currentContext, VitalFactory vitalFactory) 
            : base(currentContext, vitalFactory) { }
        
        
        public override void Increase(float amount)
        {
            // Value = Mathf.Clamp(Value * Modifier + amount, MinValue, MaxValue);
        }

        public override void Decrease(float amount)
        {
            
        }

        public override void UpdateVital(float deltaTime)
        { 
            
        }

        public override void ResetVital()
        {
            
        }
        
    }
}