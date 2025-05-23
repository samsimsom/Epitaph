using UnityEngine;
using System.Collections;

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
            Ctx.IsCrouching = true;
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
            if (Ctx.PlayerController.PlayerInput.IsCrouchPressedThisFrame && CanStandUp())
            {
                Ctx.IsCrouching = false;

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

            // Çömelirken zıplama
            if (Ctx.PlayerController.PlayerInput.IsJumpPressed &&
                Ctx.PlayerController.CharacterController.isGrounded && CanStandUp())
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
            Ctx.AppliedMovementX = input.x * Ctx.CrouchSpeed;
            Ctx.AppliedMovementZ = input.y * Ctx.CrouchSpeed;
        }

        #endregion

        #region Transition Methods

        private void TransitionCrouchState(bool crouch)
        {
            if (_crouchTransitionCoroutine != null)
                Ctx.PlayerController.StopCoroutine(_crouchTransitionCoroutine);

            if (crouch)
            {
                UpdateCameraHeightSmooth(true);
                _crouchTransitionCoroutine = Ctx.PlayerController.StartCoroutine(SmoothCrouchTransition(
                    Ctx.PlayerController.CharacterController.height, Ctx.CrouchHeight,
                    Ctx.PlayerController.CharacterController.center, Ctx.CrouchControllerCenter,
                    Ctx.CrouchTransitionDuration, true));
            }
            else
            {
                if (CanStandUp())
                {
                    UpdateCameraHeightSmooth(false);
                    _crouchTransitionCoroutine = Ctx.PlayerController.StartCoroutine(SmoothCrouchTransition(
                        Ctx.PlayerController.CharacterController.height, Ctx.NormalHeight,
                        Ctx.PlayerController.CharacterController.center, Ctx.NormalControllerCenter,
                        Ctx.CrouchTransitionDuration, false));
                }
                else
                {
                    Ctx.IsCrouching = true;
                    UpdateCameraHeightSmooth(true);
                }
            }
        }

        private void UpdateCameraHeightSmooth(bool crouch)
        {
            if (_cameraTransitionCoroutine != null)
                Ctx.PlayerController.StopCoroutine(_cameraTransitionCoroutine);

            var targetY = crouch ? Ctx.CrouchCameraHeight : Ctx.NormalCameraHeight;
            _cameraTransitionCoroutine = Ctx.PlayerController.StartCoroutine(
                SmoothCameraTransition(targetY, Ctx.CrouchTransitionDuration));
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
                Debug.Log("Cannot stand up, hit: " + hit.collider.name);
                return false;
            }
            return true;
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region Coroutines

        private IEnumerator SmoothCrouchTransition(float fromHeight, float toHeight,
            Vector3 fromCenter, Vector3 toCenter, float duration, bool crouching)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                Ctx.PlayerController.CharacterController.height = Mathf.Lerp(fromHeight, toHeight, t);
                Ctx.PlayerController.CharacterController.center = Vector3.Lerp(fromCenter, toCenter, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Ctx.PlayerController.CharacterController.height = toHeight;
            Ctx.PlayerController.CharacterController.center = toCenter;
            Ctx.IsCrouching = crouching;
        }

        private IEnumerator SmoothCameraTransition(float targetY, float duration)
        {
            var cameraTransform = Ctx.PlayerController.CameraTransform;
            var startPos = cameraTransform.localPosition;
            var endPos = new Vector3(startPos.x, targetY, startPos.z);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                cameraTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cameraTransform.localPosition = endPos;
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}