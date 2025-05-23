namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerStateFactory
    {
        private PlayerStateMachine _context;

        public PlayerStateFactory(PlayerStateMachine currentContext)
        {
            _context = currentContext;
        }

        public PlayerBaseState Idle() => new PlayerIdleState(_context, this);
        public PlayerBaseState Walk() => new PlayerWalkState(_context, this);
        public PlayerBaseState Run() => new PlayerRunState(_context, this);
        public PlayerBaseState Jump() => new PlayerJumpState(_context, this);
        public PlayerBaseState Crouch() => new PlayerCrouchState(_context, this);
        // Gelecekte eklenebilecek diğer state'ler için metotlar...
    }
}