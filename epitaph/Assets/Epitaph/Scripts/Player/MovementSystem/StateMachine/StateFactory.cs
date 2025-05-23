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
        public BaseState Run() => new RunState(_context, this);
        public BaseState Jump() => new JumpState(_context, this);
        public BaseState Crouch() => new CrouchState(_context, this);
        
        // Gelecekte eklenebilecek diğer state'ler için metotlar...
    }
}