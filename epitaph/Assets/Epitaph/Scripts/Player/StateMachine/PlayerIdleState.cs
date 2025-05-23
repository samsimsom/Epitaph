using UnityEngine;

namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerStateMachine currentContext, 
            PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            Debug.Log("IDLE: Enter");
            Ctx.AppliedMovementX = 0;
            Ctx.AppliedMovementZ = 0;
            // _ctx.Animator.SetBool("IsWalking", false); // Animasyon
            // _ctx.Animator.SetBool("IsRunning", false);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void ExitState()
        {
            Debug.Log("IDLE: Exit");
        }

        public override void CheckSwitchStates()
        {
            if (Ctx.IsCrouchPressedThisFrame)
            {
                SwitchState(Factory.Crouch());
            }
            else if (Ctx.IsJumpPressed && Ctx.CharacterController.isGrounded)
            {
                SwitchState(Factory.Jump());
            }
            else if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
            {
                SwitchState(Factory.Run());
            }
            else if (Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Walk());
            }
        }

        private void SwitchState(PlayerBaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}