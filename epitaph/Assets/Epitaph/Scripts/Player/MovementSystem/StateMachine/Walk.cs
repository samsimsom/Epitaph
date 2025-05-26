using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Walk : StateBase
    {
        public Walk(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // Debug.Log("WALK: Enter");
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }
        
        public override void FixedUpdateState() { }
        
        public override void ExitState()
        {
            // Debug.Log("WALK: Exit");
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
                     Ctx.HasObstacleAboveForJump())
            {
                SwitchState(Factory.Jump());
            }
            else if (!Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                SwitchState(Factory.Idle());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
                     Ctx.PlayerController.PlayerInput.IsRunPressed)
            {
                SwitchState(Factory.Run());
            }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.AppliedMovementX = Mathf.Lerp(Ctx.AppliedMovementX, input.x * Ctx.WalkSpeed, 0.1f);
            Ctx.AppliedMovementZ = Mathf.Lerp(Ctx.AppliedMovementZ, input.y * Ctx.WalkSpeed, 0.1f);
        }

        private void SwitchState(StateBase @new)
        {
            ExitState();
            @new.EnterState();
            Ctx.Current = @new;
        }
    }
}