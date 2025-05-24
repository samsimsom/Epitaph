using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class IdleState : BaseState
    {
        public IdleState(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }
        
        public override void EnterState()
        {
            // Debug.Log("IDLE: Enter");
            
            Ctx.AppliedMovementX = 0;
            Ctx.AppliedMovementZ = 0;
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            // Debug.Log("IDLE: Exit");
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame)
            {
                SwitchState(Factory.Crouch());
            }
            else if (Ctx.PlayerController.PlayerInput.IsJumpPressedThisFrame && 
                     Ctx.CoyoteTimeCounter > 0f &&
                     !Ctx.HasObstacleAboveForJump())
            {
                SwitchState(Factory.Jump());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput 
                     && Ctx.PlayerController.PlayerInput.IsRunPressed)
            {
                SwitchState(Factory.Run());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                SwitchState(Factory.Walk());
            }
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}