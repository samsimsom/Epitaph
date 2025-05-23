using UnityEngine;
    
namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerWalkState : PlayerBaseState
    {
        public PlayerWalkState(PlayerStateMachine currentContext, 
            PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            // Debug.Log("WALK: Enter");
            // _ctx.Animator.SetBool("IsWalking", true);
            // _ctx.Animator.SetBool("IsRunning", false);
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
            if (Ctx.IsCrouchPressed)
            {
                // SwitchState(_factory.Crouch());
            }
            else if (Ctx.IsJumpPressed && Ctx.CharacterController.isGrounded)
            {
                // SwitchState(_factory.Jump());
            }
            else if (!Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Idle());
            }
            else if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
            {
                // SwitchState(_factory.Run());
            }
        }

        private void HandleMovementInput()
        {
            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed;
        }

        private void SwitchState(PlayerBaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}