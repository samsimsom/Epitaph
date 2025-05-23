namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class IdleState : BaseState
    {
        public IdleState(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }
    }
}