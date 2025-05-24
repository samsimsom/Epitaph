namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public abstract class BaseState
    {
        public string StateName => GetType().Name;
        protected MovementBehaviour Ctx;
        protected StateFactory Factory;

        public BaseState(MovementBehaviour currentContext, StateFactory stateFactory)
        {
            Ctx = currentContext;
            Factory = stateFactory;
        }

        public virtual void EnterState() {  }
        
        public virtual void UpdateState() {  }
        public virtual void FixedUpdateState() {  }
        
        public virtual void ExitState() {  }
        
        public virtual void CheckSwitchStates() {  }
        public virtual void InitializeSubState() {  }
    }
}