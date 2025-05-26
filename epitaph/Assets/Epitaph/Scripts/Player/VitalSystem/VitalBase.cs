
namespace Epitaph.Scripts.Player.VitalSystem
{
    public abstract class VitalBase
    {
        protected VitalBehaviour Ctx;
        protected VitalFactory Factory;
        
        public string StateName => GetType().Name;
        
        public abstract float Value { get; set; }
        public abstract float MinValue { get; set; }
        public abstract float MaxValue { get; set; }
        
        public abstract float BaseIncreaseRate { get; set; }
        public abstract float BaseDecreaseRate { get; set; }
        public abstract float Modifier { get; set; }
        
        public VitalBase(VitalBehaviour currentContext, VitalFactory vitalFactory)
        {
            Ctx = currentContext;
            Factory = vitalFactory;
        }

        public abstract void Increase(float amount);
        public abstract void Decrease(float amount);
        public abstract void UpdateVital(float deltaTime);
        public abstract void ResetVital();
    }
}