using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Run : StateBase
    {
        public Run(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }
        
        public override void EnterState()
        {
            // Debug.Log("RUN: Enter");
            if (!Ctx.CanRun())
            {
                SwitchState(Factory.Walk());
                return;
            }
            
            Ctx.IsRunning = true;
        }

        public override void UpdateState()
        {
            if (!Ctx.CanRun())
            {
                SwitchState(Factory.Walk());
                return;
            }
            
            HandleMovementInput();
            CheckSwitchStates();
        }
        public override void FixedUpdateState() { }


        public override void ExitState()
        {
            // Debug.Log("RUN: Exit");
            Ctx.IsRunning = false;
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
                     Ctx.CanJump())
            {
                SwitchState(Factory.Jump());
            }
            else if (!Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                SwitchState(Factory.Idle());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
                     !Ctx.PlayerController.PlayerInput.IsRunPressed)
            {
                SwitchState(Factory.Walk());
            }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.AppliedMovementX = Mathf.Lerp(Ctx.AppliedMovementX, 
                input.x * Ctx.RunSpeed, Ctx.SpeedTransitionDuration);
            Ctx.AppliedMovementZ = Mathf.Lerp(Ctx.AppliedMovementZ, 
                input.y * Ctx.RunSpeed, Ctx.SpeedTransitionDuration);
        }

        private void SwitchState(StateBase @new)
        {
            ExitState();
            @new.EnterState();
            Ctx.CurrentState = @new;
        }
    }
}