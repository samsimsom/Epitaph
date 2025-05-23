using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class JumpState : BaseState
    {
        public JumpState(MovementBehaviour currentContext, StateFactory stateFactory) 
            : base(currentContext, stateFactory) { }

        public override void EnterState()
        {
            Debug.Log("JUMP: Enter");
            
            Ctx.CurrentMovementY = Ctx.JumpForce;
            Ctx.AppliedMovementX = Ctx.PlayerController.PlayerInput.MoveInput.x * Ctx.WalkSpeed;
            Ctx.AppliedMovementZ = Ctx.PlayerController.PlayerInput.MoveInput.y * Ctx.WalkSpeed;
            
            // Ctx.IsJumpPressed = false;
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
            Debug.Log("JUMP: Exit");
            // Eğer zıplama animasyonu bool ise burada false yapın
        }

        public override void InitializeSubState()
        {
        }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde ve dikey hız negatif veya sıfıra yakınsa
            if (Ctx.PlayerController.CharacterController.isGrounded && Ctx.CurrentMovementY <= 0)
            {
                if (Ctx.IsCrouchPressedThisFrame || Ctx.IsCrouching) // Zıplamadan sonra crouch'a düşebilir
                {
                    // SwitchState(Factory.Crouch());
                }
                else if (Ctx.PlayerController.PlayerInput.IsMoveInput && Ctx.IsRunPressed)
                {
                    // SwitchState(Factory.Run());
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
            // Havadaki hızı karadaki hızdan farklı olabilir (örneğin walkspeed'in %80'i)
            var airControlFactor = 0.8f;
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