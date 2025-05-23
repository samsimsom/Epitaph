using UnityEngine;

namespace Epitaph.Scripts.Player.StateMachine
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(PlayerStateMachine currentContext,
            PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
        }

        public override void EnterState()
        {
            Debug.Log("CROUCH: Enter");
            Ctx.IsCrouching = true;
            Ctx.ChangeCharacterControllerDimensions(true);
            // Ctx.Animator.SetBool("IsCrouching", true);
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }
        
        public override void ExitState()
        {
            Debug.Log("CROUCH: Exit");
            // Ayağa kalkmadan önce CanStandUp kontrolü PlayerStateMachine'de yapılıyor
            Ctx.ChangeCharacterControllerDimensions(false); // Bu metot zaten IsCrouching'i false yapar (eğer kalkabilirse)
            // Ctx.Animator.SetBool("IsCrouching", false);
        }
        
        public override void CheckSwitchStates()
        {
            // Crouch'tan çıkma
            if (Ctx.IsCrouchPressedThisFrame && Ctx.CanStandUp())
            {
                Ctx.IsCrouching = false; // State machine'e artık crouch'ta olmadığımızı bildir.
                
                // ExitState'teki ChangeCharacterControllerDimensions halledecek.
                if (Ctx.IsMovementPressed && Ctx.IsRunPressed)
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
                return; // State değişti, daha fazla kontrol yapma
            }

            // Crouch'tayken zıplama (eğer izin veriliyorsa)
            if (Ctx.IsJumpPressed && Ctx.CharacterController.isGrounded && Ctx.CanStandUp())
            {
                Ctx.IsCrouching = false;
                SwitchState(Factory.Jump());
                return;
            }

            // Crouch'tayken hareket ediyorsa ve koşma tuşuna basılırsa ne olacağı (örneğin normal koşuya geçebilir)
            // if (Ctx.IsMovementPressed && Ctx.IsRunPressed && Ctx.CanStandUp())
            // {
            //    Ctx.IsCrouching = false;
            //    SwitchState(Factory.Run());
            //    return;
            // }
        }

        private void HandleMovementInput()
        {
            var input = Ctx.CurrentMovementInput;
            Ctx.AppliedMovementX = input.x * Ctx.CrouchSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.CrouchSpeed;
        }

        private void SwitchState(PlayerBaseState newState)
        {
            ExitState(); // Önce mevcut durumdan çık
            newState.EnterState(); // Sonra yeni duruma gir
            Ctx.CurrentState = newState;
        }
    }
}