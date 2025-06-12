using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class LocomotionHandler : MovementSubBehaviour
    {
        public LocomotionHandler(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController)
        {
        }

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
            movement.y = MovementBehaviour.VerticalMovement;
            
            PlayerController.CharacterController.Move(movement * Time.deltaTime);
        }
    }
}