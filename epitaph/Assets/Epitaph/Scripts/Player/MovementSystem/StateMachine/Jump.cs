using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Jump : StateBase
    {
        public Jump(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // Debug.Log("JUMP: Enter");
            
            Ctx.VerticalMovement = Ctx.JumpForce;
            Ctx.IsJumping = true;
        }

        public override void UpdateState()
        {
            HandleAirborneMovement();
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }
        
        public override void ExitState()
        {
            // Debug.Log("JUMP: Exit");
            Ctx.IsJumping = false;
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde ve dikey hız negatif veya sıfıra yakınsa
            if (Ctx.IsGrounded && Ctx.CapsulVelocity.y <= 0)
            {
                if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame || 
                    Ctx.IsCrouching)
                {
                    SwitchState(Factory.Crouch());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
                         Ctx.PlayerController.PlayerInput.IsRunPressed)
                {
                    SwitchState(Factory.Run());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput)
                {
                    SwitchState(Factory.Walk());
                }
                else
                {
                    SwitchState(Factory.Idle());
                }
            }
            else
            {
                if (Ctx.IsFalling)
                {
                    Ctx.IsJumping = false;
                    // Debug.Log("Fall State");
                    // SwitchState(Factory.Idle());
                }
            }
        }

        private void HandleAirborneMovement()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            var airControlFactor = Ctx.AirControlFactor;
            
            Ctx.AppliedMovementX = input.x * (Ctx.CurrentState is Run ? 
                Ctx.RunSpeed * airControlFactor : Ctx.WalkSpeed * airControlFactor);
            Ctx.AppliedMovementZ = input.y * (Ctx.CurrentState is Run ? 
                Ctx.RunSpeed * airControlFactor : Ctx.WalkSpeed* airControlFactor);
        }

        private void SwitchState(StateBase @new)
        {
            ExitState();
            @new.EnterState();
            Ctx.CurrentState = @new;
        }
    }
}