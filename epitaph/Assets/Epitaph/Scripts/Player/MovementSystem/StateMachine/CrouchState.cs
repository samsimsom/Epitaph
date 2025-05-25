using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem.StateMachine
{
    public class CrouchState : BaseState
    {
        #region Fields

        private Coroutine _crouchTransitionCoroutine;
        private Coroutine _cameraTransitionCoroutine;

        #endregion

        #region Constructor

        public CrouchState(MovementBehaviour currentContext, StateFactory stateFactory)
            : base(currentContext, stateFactory) { }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region State Events

        public override void EnterState()
        {
            TransitionCrouchState(true);
        }

        public override void UpdateState()
        {
            HandleMovementInput();
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }
        
        public override void ExitState()
        {
            TransitionCrouchState(false);
        }

        public override void InitializeSubState() { }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region State Switch

        public override void CheckSwitchStates()
        {
            // Çömelmeden çıkma
            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame && 
                CanStandUp())
            {
                Ctx.IsCrouching = false;

                if (Ctx.PlayerController.PlayerInput.IsMoveInput && 
                    Ctx.PlayerController.PlayerInput.IsRunPressed)
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
            } else if (!Ctx.PlayerController.PlayerInput.IsMoveInput)
            {
                var input = Ctx.PlayerController.PlayerInput.MoveInput;
                Ctx.AppliedMovementX = input.x * Ctx.CrouchSpeed;
                Ctx.AppliedMovementZ =input.y * Ctx.CrouchSpeed;
            }

            // Çömelirken zıplama
            if (Ctx.PlayerController.PlayerInput.IsJumpPressedThisFrame &&
                Ctx.IsGrounded && 
                CanStandUp() && 
                Ctx.HasObstacleAboveForJump())
            {
                Ctx.IsCrouching = false;
                SwitchState(Factory.Jump());
            }
        }

        private void SwitchState(BaseState newState)
        {
            ExitState();
            newState.EnterState();
            Ctx.CurrentState = newState;
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        // ---------------------------------------------------------------------------- //
        
        #region Input Handling

        private void HandleMovementInput()
        {
            var input = Ctx.PlayerController.PlayerInput.MoveInput;
            Ctx.AppliedMovementX = Mathf.Lerp(Ctx.AppliedMovementX, input.x * Ctx.CrouchSpeed, 0.1f);
            Ctx.AppliedMovementZ = Mathf.Lerp(Ctx.AppliedMovementZ, input.y * Ctx.CrouchSpeed, 0.1f);
        }

        #endregion

        #region Transition Methods

        private void TransitionCrouchState(bool crouch)
        {
            UpdateCameraHeightSmooth(crouch);
    
            if (crouch)
            {
                Ctx.PlayerController.CharacterController.height = Ctx.CrouchHeight;
                Ctx.PlayerController.CharacterController.center = Ctx.CrouchControllerCenter;
            }
            else if (CanStandUp())
            {
                Ctx.PlayerController.CharacterController.height = Ctx.NormalHeight;
                Ctx.PlayerController.CharacterController.center = Ctx.NormalControllerCenter;
            }
    
            Ctx.IsCrouching = crouch;
        }

        private void UpdateCameraHeightSmooth(bool crouch)
        {
            var targetY = crouch ? Ctx.CrouchCameraHeight : Ctx.NormalCameraHeight;
            
            // ViewBehaviour üzerinden kamera yüksekliğini ayarla
            var viewBehaviour = Ctx.PlayerController.ViewBehaviour;

            if (viewBehaviour == null)
            {
                Debug.LogError($"PlayerController ViewBehaviour is null in CrouchState. " +
                               $"UpdateCameraHeightSmooth | {viewBehaviour}");
                return;
            }
            
            Ctx.PlayerController.ViewBehaviour.SetCameraHeight(targetY);
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region Utilities

        private bool CanStandUp()
        {
            var radius = Ctx.PlayerController.CharacterController.radius;
            var castDistance = Ctx.NormalHeight - Ctx.CrouchHeight;
            var castStartPoint = Ctx.PlayerController.CharacterController.transform.position +
                                 Ctx.CrouchControllerCenter + Vector3.up * (Ctx.CrouchHeight / 2 - radius);

            Debug.DrawRay(castStartPoint, Vector3.up * castDistance, Color.red, 2f);
            if (Physics.SphereCast(castStartPoint, radius, Vector3.up,
                out var hit, castDistance, ~LayerMask.GetMask("Player")))
            {
                Debug.Log($"<color=orange>Cannot stand up</color>, hit: {hit.collider.name}");
                return false;
            }
            return true;
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
    }
}