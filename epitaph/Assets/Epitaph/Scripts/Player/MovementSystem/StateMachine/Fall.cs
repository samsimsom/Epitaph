namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Fall : StateBase
    {
        
        private bool _wasRunningBeforeJump;

        public Fall(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // Zıplamadan önce koşuyor muydu?
            _wasRunningBeforeJump = Ctx.PlayerController.PlayerInput.IsRunPressed;
            
            Ctx.FallHandler.IsFalling = true;
            
            // Fall state'e girdiğinde jump state'den geliyorsak IsJumping'i false yap
            if (Ctx.JumpHandler.IsJumping)
            {
                Ctx.JumpHandler.IsJumping = false;
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
            Ctx.FallHandler.IsFalling = false;
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde fall state'den çık
            if (Ctx.GroundHandler.IsGrounded && Ctx.GravityHandler.VerticalMovement <= 0)
            {
                if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame && Ctx.PlayerController.PlayerInput.IsCrouchPressed || Ctx.CrouchHandler.IsCrouching)
                {
                    Ctx.StateManager.SwitchState(Factory.Crouch());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
                         Ctx.PlayerController.PlayerInput.IsRunPressed)
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
            // Eğer havadayken jump tuşuna basılırsa ve zıplayabiliyorsa
            else if (Ctx.PlayerController.PlayerInput.IsJumpPressedThisFrame && 
                     Ctx.JumpHandler.CanJump())
            {
                Ctx.StateManager.SwitchState(Factory.Jump());
            }
        }

        private void HandleAirborneMovement()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            
            // Fall state'de daha az air control (jump'tan daha az)
            var fallAirControlFactor = Ctx.JumpHandler.AirControlFactor * 0.8f;
            
            // Zıplamadan önce koşuyorsa veya şu anda run tuşuna basılıyorsa run hızını kullan
            var baseSpeed = (_wasRunningBeforeJump || Ctx.PlayerController.PlayerInput.IsRunPressed) 
                ? Ctx.LocomotionHandler.RunSpeed 
                : Ctx.LocomotionHandler.WalkSpeed;
            
            Ctx.LocomotionHandler.AppliedMovementX = input.x * baseSpeed * fallAirControlFactor;
            Ctx.LocomotionHandler.AppliedMovementZ = input.y * baseSpeed * fallAirControlFactor;
            
        }
    }
}