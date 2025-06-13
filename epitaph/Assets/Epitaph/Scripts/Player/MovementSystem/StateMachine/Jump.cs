namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Jump : StateBase
    {
        private bool _wasRunningBeforeJump;
        
        public Jump(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // JumpHandler üzerinden zıplama eylemini çağır.
            if (Ctx.JumpHandler.CanJump())
            {
                // Zıplamadan önce koşuyor muydu?
                _wasRunningBeforeJump = Ctx.PlayerController.PlayerInput.IsRunPressed;
                Ctx.JumpHandler.PerformJump();
            }
        }

        public override void UpdateState()
        {
            HandleAirborneMovement();
            CheckSwitchStates();
        }
        
        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            Ctx.JumpHandler.IsJumping = false;
        }
        
        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde ve dikey hız negatif veya sıfıra yakınsa
            if (Ctx.GroundHandler.IsGrounded && Ctx.LocomotionHandler.CapsulVelocity.y <= 0 && Ctx.GravityHandler.VerticalMovement <= 0)
            {
                if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame && Ctx.PlayerController.PlayerInput.IsCrouchPressed || Ctx.CrouchHandler.IsCrouching)
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
            else if (Ctx.FallHandler.IsFalling && Ctx.GravityHandler.VerticalMovement < 0)
            {
                Ctx.StateManager.SwitchState(Factory.Fall());
            }
        }
        
        private void HandleAirborneMovement()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            var airControlFactor = Ctx.JumpHandler.AirControlFactor;
            
            // Zıplamadan önce koşuyorsa veya şu anda run tuşuna basılıyorsa run hızını kullan
            var baseSpeed = (_wasRunningBeforeJump || Ctx.PlayerController.PlayerInput.IsRunPressed) 
                ? Ctx.LocomotionHandler.RunSpeed 
                : Ctx.LocomotionHandler.WalkSpeed;
            
            Ctx.LocomotionHandler.AppliedMovementX = input.x * baseSpeed * airControlFactor;
            Ctx.LocomotionHandler.AppliedMovementZ = input.y * baseSpeed * airControlFactor;
        }
    }
}