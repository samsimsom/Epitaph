
namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class JumpState : BaseState
    {
        public JumpState(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            // Debug.Log("JUMP: Enter");
            
            Ctx.CurrentMovementY = Ctx.JumpForce;
            Ctx.AppliedMovementX = Ctx.PlayerController.PlayerInput.MoveInput.x * Ctx.WalkSpeed;
            Ctx.AppliedMovementZ = Ctx.PlayerController.PlayerInput.MoveInput.y * Ctx.WalkSpeed;
        }

        public override void UpdateState()
        {
            HandleAirborneMovement();
            CheckSwitchStates();
        }

        public override void FixedUpdateState()
        {
        }


        public override void ExitState()
        {
            // Debug.Log("JUMP: Exit");
        }

        public override void InitializeSubState()
        {
        }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde ve dikey hız negatif veya sıfıra yakınsa
            if (Ctx.PlayerController.CharacterController.isGrounded && Ctx.CurrentMovementY <= 0)
            {
                if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame || Ctx.IsCrouching)
                {
                    SwitchState(Factory.Crouch());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput 
                         && Ctx.PlayerController.PlayerInput.IsRunPressed)
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
        }

        private void HandleAirborneMovement()
        {
            // Havada bir miktar kontrol sağlamak için
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            var airControlFactor = 1.25f;
            Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed * airControlFactor;
            Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed * airControlFactor;
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}