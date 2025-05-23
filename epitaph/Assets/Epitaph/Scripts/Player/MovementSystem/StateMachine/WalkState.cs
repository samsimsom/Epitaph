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
                // SwitchState(Factory.Crouch());
            }
            else if (Ctx.IsJumpPressed && Ctx.PlayerController.CharacterController.isGrounded)
            {
                // SwitchState(Factory.Jump());
            }
            else if (!Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Idle());
            }
            else if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
            {
                // SwitchState(Factory.Run());
            }
        }

        private void HandleMovementInput()
        {
            // var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            // Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed;
            // Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed;
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}