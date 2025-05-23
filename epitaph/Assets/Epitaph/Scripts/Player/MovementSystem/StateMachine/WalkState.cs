using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class WalkState : BaseState
    {
        public WalkState(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            Debug.Log("WALK: Enter");
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }
        
        public override void FixedUpdateState() { }
        
        public override void ExitState()
        {
            Debug.Log("WALK: Exit");
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            if (Ctx.IsCrouchPressedThisFrame)
            {
                Debug.Log("Crouch");
                // SwitchState(Factory.Crouch());
            }
            else if (Ctx.PlayerController.PlayerInput.IsJumpPressed && Ctx.PlayerController.CharacterController.isGrounded)
            {
                SwitchState(Factory.Jump());
            }
            else if (!Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                SwitchState(Factory.Idle());
            }
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput && Ctx.IsRunPressed)
            {
                Debug.Log("Run");
                // SwitchState(Factory.Run());
            }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed;
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}