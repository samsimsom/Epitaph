using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class LocomotionHandler : MovementSubBehaviour
    {
        public float WalkSpeed = 2.5f;
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 1.5f;
        public float SpeedTransitionDuration = 0.1f;
        public float IdleTransitionDuration = 0.25f;

        public LocomotionHandler(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController) { }

        public override void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (PlayerController.CharacterController == null) return;

            MovementBehaviour.CapsulVelocity = PlayerController.CharacterController.velocity;
            MovementBehaviour.CurrentSpeed = new Vector3(MovementBehaviour.CapsulVelocity.x, 0, MovementBehaviour.CapsulVelocity.z).magnitude;
                
            var moveInput = new Vector2(MovementBehaviour.AppliedMovementX, MovementBehaviour.AppliedMovementZ);
            MovementBehaviour.StepHandler.HandleStepOffset(moveInput);

            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (PlayerController.CharacterController == null) return;
    
            var cameraTransform = PlayerController.PlayerCamera.transform;
            var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
            
            var movement = cameraRight * MovementBehaviour.AppliedMovementX + cameraForward * MovementBehaviour.AppliedMovementZ;

            // Tırmanılamayan yüzeylerde hareketi engellemek için eğim kontrolü
            // if (MovementBehaviour.IsGrounded)
            // {
            //     var slopeAngle = Vector3.Angle(Vector3.up, MovementBehaviour.GroundNormal);
            //     if (slopeAngle > PlayerController.CharacterController.slopeLimit)
            //     {
            //         // Hareketi eğim düzlemine yansıtarak tırmanmayı engelle ve kaymasını sağla
            //         var horizontalMovement = new Vector3(movement.x, 0, movement.z);
            //         var projectedMovement = Vector3.ProjectOnPlane(horizontalMovement, MovementBehaviour.GroundNormal);
            //
            //         movement.x = projectedMovement.x;
            //         movement.z = projectedMovement.z;
            //
            //         // Eğer zıplamıyorsak ve StepHandler'dan kaynaklanan bir yükselme varsa bunu engelle
            //         if (MovementBehaviour.VerticalMovement > 0 && !MovementBehaviour.IsJumping)
            //         {
            //             MovementBehaviour.VerticalMovement = 0;
            //         }
            //     }
            // }
            
            movement.y = MovementBehaviour.VerticalMovement;
            
            PlayerController.CharacterController.Move(movement * Time.deltaTime);
        }
    }
}