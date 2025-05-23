using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class RunState : BaseState
    {
        public RunState(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }
        
        public override void EnterState()
        {
            Debug.Log("RUN: Enter");
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }
        public override void FixedUpdateState() { }


        public override void ExitState()
        {
            Debug.Log("RUN: Exit");
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame)
            {
                // Koşarken crouch'a geçince ne olacağına karar verin.
                // Belki direkt crouch walk veya slide? Şimdilik normal crouch.
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
            else if (Ctx.PlayerController.PlayerInput.IsMoveInput && !Ctx.PlayerController.PlayerInput.IsRunPressed)
            {
                SwitchState(Factory.Walk());
            }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.AppliedMovementX = input.x * Ctx.RunSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.RunSpeed;
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}