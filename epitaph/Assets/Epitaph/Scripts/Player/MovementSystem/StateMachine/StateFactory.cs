namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class StateFactory
    {
        private MovementBehaviour _context;

        public StateFactory(MovementBehaviour currentContext)
        {
            _context = currentContext;
        }

        public StateBase Idle() => new Idle(_context, this);
        public StateBase Walk() => new Walk(_context, this);
        public StateBase Run() => new Run(_context, this);
        public StateBase Jump() => new Jump(_context, this);
        public StateBase Fall() => new Fall(_context, this);
        public StateBase Crouch() => new Crouch(_context, this);
        
        // Gelecekte eklenebilecek diğer state'ler için metotlar...
    }
}