namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Jump : StateBase
    {
        public Jump(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // JumpHandler üzerinden zıplama eylemini çağır.
            if (Ctx.JumpHandler.CanJump())
            {
                Ctx.JumpHandler.PerformJump();
            }
        }

        public override void UpdateState()
        {
            HandleAirborneMovement();
            CheckSwitchStates();
        }
        
        // public override void FixedUpdateState() { }

        public override void ExitState()
        {
            Ctx.IsJumping = false;
        }
        
        // public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde ve dikey hız negatif veya sıfıra yakınsa
            if (Ctx.IsGrounded && Ctx.CapsulVelocity.y <= 0 && Ctx.VerticalMovement <= 0)
            {
                if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame || Ctx.IsCrouching)
                {
                    Ctx.StateManager.SwitchState(Factory.Crouch());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput && Ctx.PlayerController.PlayerInput.IsRunPressed)
                {
                    Ctx.StateManager.SwitchState(Factory.Run());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput)
                {
                    Ctx.StateManager.SwitchState(Factory.Walk());
                }
                else
                {
                    Ctx.StateManager.SwitchState(Factory.Idle());
                }
            }
            // Jump'tan Fall'a geçiş - dikey hız negatif olduğunda
            else if (Ctx.IsFalling && Ctx.VerticalMovement < 0)
            {
                Ctx.StateManager.SwitchState(Factory.Fall());
            }
        }
        
        private void HandleAirborneMovement()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            var airControlFactor = Ctx.JumpHandler.AirControlFactor;
            
            Ctx.AppliedMovementX = input.x * (Ctx.StateManager.CurrentState is Run ? Ctx.LocomotionHandler.RunSpeed * airControlFactor : Ctx.LocomotionHandler.WalkSpeed * airControlFactor);
            Ctx.AppliedMovementZ = input.y * (Ctx.StateManager.CurrentState is Run ? Ctx.LocomotionHandler.RunSpeed * airControlFactor : Ctx.LocomotionHandler.WalkSpeed* airControlFactor);
        }
    }
}