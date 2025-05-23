namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class StateFactory
    {
        private MovementBehaviour _context;

        public StateFactory(MovementBehaviour currentContext)
        {
            _context = currentContext;
        }

        public BaseState Idle() => new IdleState(_context, this);
        public BaseState Walk() => new WalkState(_context, this);
        // public PlayerBaseState Run() => new PlayerRunState(_context, this);
        // public PlayerBaseState Jump() => new PlayerJumpState(_context, this);
        // public PlayerBaseState Crouch() => new PlayerCrouchState(_context, this);
        // Gelecekte eklenebilecek diğer state'ler için metotlar...
    }
}