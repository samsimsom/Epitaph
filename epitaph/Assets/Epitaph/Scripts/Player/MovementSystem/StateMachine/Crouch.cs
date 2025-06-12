using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class Crouch : StateBase
    {
        public Crouch(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            Ctx.CrouchHandler.HandleCrouch();
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            // Bu state'den çıkıldığında IsCrouching true ise, bu, oyuncunun
            // ayağa kalkmaya çalıştığı anlamına gelir.
            if (Ctx.IsCrouching)
            {
                Ctx.CrouchHandler.HandleStandUp();
            }
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            if (!Ctx.IsGrounded && Ctx.IsFalling)
            {
                Ctx.StateManager.SwitchState(Factory.Fall());
                return;
            }

            // Oyuncu çömelme tuşuna basmıyorsa VE ayağa kalkabiliyorsa, normal state'e dön
            if (!Ctx.PlayerController.PlayerInput.IsCrouchPressed && Ctx.CrouchHandler.CanStandUp())
            {
                if (Ctx.PlayerController.PlayerInput.IsMoveInput && Ctx.PlayerController.PlayerInput.IsRunPressed)
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
        }

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            
            Ctx.AppliedMovementX = Mathf.Lerp(Ctx.AppliedMovementX, 
                input.x * Ctx.CrouchSpeed, Ctx.SpeedTransitionDuration);
            Ctx.AppliedMovementZ = Mathf.Lerp(Ctx.AppliedMovementZ, 
                input.y * Ctx.CrouchSpeed, Ctx.SpeedTransitionDuration);
        }
    }
}