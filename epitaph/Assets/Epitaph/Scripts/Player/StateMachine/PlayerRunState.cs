using UnityEngine;

namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerRunState : PlayerBaseState
    {
        public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            Debug.Log("RUN: Enter");
            // Ctx.Animator.SetBool("IsWalking", false); // veya IsWalking'i de true yapıp hızı artırabilirsiniz
            // Ctx.Animator.SetBool("IsRunning", true);
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }

        public override void ExitState()
        {
            Debug.Log("RUN: Exit");
        }

        public override void CheckSwitchStates()
        {
            if (Ctx.IsCrouchPressedThisFrame)
            {
                // Koşarken crouch'a geçince ne olacağına karar verin.
                // Belki direkt crouch walk veya slide? Şimdilik normal crouch.
                SwitchState(Factory.Crouch());
            }
            else if (Ctx.IsJumpPressed && Ctx.CharacterController.isGrounded)
            {
                SwitchState(Factory.Jump());
            }
            else if (!Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Idle());
            }
            else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
            {
                SwitchState(Factory.Walk());
            }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.CurrentMovementInput;
            Ctx.AppliedMovementX = input.x * Ctx.RunSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.RunSpeed;
        }

        private void SwitchState(PlayerBaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}