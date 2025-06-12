using Epitaph.Scripts.Player.MovementSystem;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Jump : StateBase
    {
        public Jump(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // JumpHandler üzerinden zıplama eylemini çağır.
            if (Ctx.JumpHandler.CanJump())
            {
                Ctx.JumpHandler.PerformJump();
            }
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }
        
        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            Ctx.IsJumping = false;
        }
        
        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Dikey hareket negatif olduğunda (yani karakter düşmeye başladığında) Fall durumuna geç.
            if (Ctx.VerticalMovement < 0.0f)
            {
                Ctx.StateManager.SwitchState(Factory.Fall());
            }
        }
    }
}