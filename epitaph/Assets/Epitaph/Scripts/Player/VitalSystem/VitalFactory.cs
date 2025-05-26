namespace Epitaph.Scripts.Player.VitalSystem
{
    public class VitalFactory
    {
        private VitalBehaviour _context;

        public VitalFactory(VitalBehaviour currentContext)
        {
            _context = currentContext;
        }
        
        // public Health Health { get; private set; }
        public VitalBase StaminaVital() => new StaminaVital(_context, this);
        // public Hunger Hunger { get; private set; }
        // public Thirst Thirst { get; private set; }
        // public Fatigue Fatigue { get; private set; }
    }
}