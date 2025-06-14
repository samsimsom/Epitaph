using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Idle : StateBase
    {
        public Idle(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }
        
        public override void EnterState()
        {
            // Debug.Log("IDLE: Enter");
        }

        public override void UpdateState()
        {
            HandleMovementInput();
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
            // Önce düşme kontrolü - en yüksek öncelik
            if (!Ctx.GroundHandler.IsGrounded && Ctx.FallHandler.IsFalling)
            {
                Ctx.StateManager.SwitchState(Factory.Fall());
                return;
            }

            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame && Ctx.PlayerController.PlayerInput.IsCrouchPressed)
            {
                Ctx.StateManager.SwitchState(Factory.Crouch());
            }
            else if (Ctx.PlayerController.PlayerInput.IsJumpPressedThisFrame && Ctx.CoyoteTimeHandler.CoyoteTimeCounter > 0f)
            {
                Ctx.StateManager.SwitchState(Factory.Jump());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput && Ctx.PlayerController.PlayerInput.IsRunPressed)
            {
                Ctx.StateManager.SwitchState(Factory.Run());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                Ctx.StateManager.SwitchState(Factory.Walk());
            }
        }
        
        private void HandleMovementInput()
        {
            // var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.LocomotionHandler.AppliedMovementX = Mathf.Lerp(Ctx.LocomotionHandler.AppliedMovementX, 0f, Ctx.LocomotionHandler.IdleTransitionDuration);
            Ctx.LocomotionHandler.AppliedMovementZ = Mathf.Lerp(Ctx.LocomotionHandler.AppliedMovementZ, 0f, Ctx.LocomotionHandler.IdleTransitionDuration);
        }
        
    }
}