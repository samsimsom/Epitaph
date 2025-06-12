using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Run : StateBase
    {
        public Run(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }
        
        public override void EnterState()
        {
            Ctx.IsRunning = true;
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }
        public override void FixedUpdateState() { }


        public override void ExitState()
        {
            Ctx.IsRunning = false;
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            if (!Ctx.IsGrounded && Ctx.IsFalling)
            {
                Ctx.StateManager.SwitchState(Factory.Fall());
                return;
            }

            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame)
            {
                Ctx.StateManager.SwitchState(Factory.Crouch());
            }
            // Zıplama kontrolünü JumpHandler'a devret
            else if (Ctx.PlayerController.PlayerInput.IsJumpPressedThisFrame && 
                     Ctx.JumpHandler.CanJump())
            {
                Ctx.StateManager.SwitchState(Factory.Jump());
            }
            else if (!Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                Ctx.StateManager.SwitchState(Factory.Idle());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
                     !Ctx.PlayerController.PlayerInput.IsRunPressed)
            {
                Ctx.StateManager.SwitchState(Factory.Walk());
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
    }
}