using UnityEngine;

namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
        }

        public override void EnterState()
        {
            Debug.Log("JUMP: Enter");
            
            Ctx.CurrentMovementY = Ctx.JumpForce;
            // Ctx.Animator.SetTrigger("Jump"); // Animasyon
            
            // Havada yönlendirme için
            var input = Ctx.CurrentMovementInput;
            Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed;
        }

        public override void UpdateState()
        {
            HandleAirborneMovement();
            CheckSwitchStates();
        }
        
        public override void ExitState()
        {
            Debug.Log("JUMP: Exit");
            // Eğer zıplama animasyonu bool ise burada false yapın
        }

        public override void CheckSwitchStates()
        {
            // Yere değdiğinde ve dikey hız negatif veya sıfıra yakınsa
            if (Ctx.CharacterController.isGrounded && Ctx.CurrentMovementY <= 0)
            {
                if (Ctx.IsCrouchPressedThisFrame || Ctx.IsCrouching) // Zıplamadan sonra crouch'a düşebilir
                {
                    SwitchState(Factory.Crouch());
                }
                else if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
                {
                    SwitchState(Factory.Run());
                }
                else if (Ctx.IsMovementPressed)
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
            var input = Ctx.CurrentMovementInput;
            
            // Havadaki hızı karadaki hızdan farklı olabilir (örneğin walkspeed'in %80'i)
            var airControlFactor = 0.8f;
            Ctx.AppliedMovementX = input.x * Ctx.WalkSpeed * airControlFactor;
            Ctx.AppliedMovementZ = input.y * Ctx.WalkSpeed * airControlFactor;
        }

        private void SwitchState(PlayerBaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }
    }
}