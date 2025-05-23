using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class CrouchState : BaseState
    {
        public CrouchState(MovementBehaviour currentContext, StateFactory stateFactory)
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            Ctx.IsCrouching = true;
            Ctx.ChangeCharacterControllerDimensions(true);
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }
        
        public override void ExitState()
        {
            Ctx.ChangeCharacterControllerDimensions(false);
        }

        public override void InitializeSubState() { }

        public override void CheckSwitchStates()
        {
            // Crouch'tan çıkma
            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame && Ctx.CanStandUp())
            {
                Ctx.IsCrouching = false; // State machine'e artık crouch'ta olmadığımızı bildir.
                
                // ExitState'teki ChangeCharacterControllerDimensions halledecek.
                if (Ctx.PlayerController.PlayerInput.IsMoveInput && Ctx.PlayerController.PlayerInput.IsRunPressed)
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
                return;
            }

            // Crouch'tayken zıplama (eğer izin veriliyorsa)
            if (Ctx.PlayerController.PlayerInput.IsJumpPressed &&
                Ctx.PlayerController.CharacterController.isGrounded && Ctx.CanStandUp())
            {
                Ctx.IsCrouching = false;
                SwitchState(Factory.Jump());
                return;
            }

            // Crouch'tayken hareket ediyorsa ve koşma tuşuna basılırsa ne olacağı
            // if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
            //     Ctx.PlayerController.PlayerInput.IsRunPressed && Ctx.CanStandUp())
            // {
            //     Ctx.IsCrouching = false;
            //     SwitchState(Factory.Run());
            // }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.AppliedMovementX = input.x * Ctx.CrouchSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.CrouchSpeed;
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}