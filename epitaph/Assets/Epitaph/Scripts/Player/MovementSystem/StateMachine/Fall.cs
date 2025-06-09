namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Fall : StateBase
    {
        public Fall(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            Ctx.IsFalling = true;
            // Fall state'e girdiğinde jump state'den geliyorsak IsJumping'i false yap
            if (Ctx.IsJumping)
            {
                Ctx.IsJumping = false;
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
            Ctx.IsFalling = false;
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde fall state'den çık
            if (Ctx.IsGrounded && Ctx.VerticalMovement <= 0)
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
            // Eğer havadayken jump tuşuna basılırsa ve coyote time varsa
            else if (Ctx.PlayerController.PlayerInput.IsJumpPressedThisFrame && 
                     Ctx.CoyoteTimeCounter > 0 && 
                     !Ctx.PlayerController.MovementBehaviour.HasObstacleAboveForJump())
            {
                Ctx.StateManager.SwitchState(Factory.Jump());
            }
        }

        private void HandleAirborneMovement()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            var airControlFactor = Ctx.AirControlFactor;
            
            // Fall state'de daha az air control (jump'tan daha az)
            var fallAirControlFactor = airControlFactor * 0.8f;
            
            Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed * fallAirControlFactor;
            Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed * fallAirControlFactor;
        }
    }
}