using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class LocomotionHandler : MovementSubBehaviour
    {
        public bool IsWalking { get; set; }
        public bool IsRunning { get; set; }
        
        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }
        public Vector3 CapsulVelocity { get; set; }
        public float CurrentSpeed { get; set; }
        
        public float WalkSpeed = 2.5f;
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 1.5f;
        public float SpeedTransitionDuration = 0.1f;
        public float IdleTransitionDuration = 0.25f;

        public LocomotionHandler(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController) { }

        public override void Start()
        {
            AdjustPlayerPosition();
        }

        public override void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (PlayerController.CharacterController == null) return;

            CapsulVelocity = PlayerController.CharacterController.velocity;
            CurrentSpeed = new Vector3(CapsulVelocity.x, 0, CapsulVelocity.z).magnitude;
                
            var moveInput = new Vector2(AppliedMovementX, AppliedMovementZ);
            MovementBehaviour.StepHandler.HandleStepOffset(moveInput);

            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (PlayerController.CharacterController == null) return;
    
            var cameraTransform = PlayerController.PlayerCamera.transform;
            var cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            var cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
            
            var movement = cameraRight * AppliedMovementX + cameraForward * AppliedMovementZ;
            
            movement.y = MovementBehaviour.GravityHandler.VerticalMovement;
            
            PlayerController.CharacterController.Move(movement * Time.deltaTime);
        }
        
        
        private void AdjustPlayerPosition()
        {
            if (PlayerController.CharacterController != null)
            {
                var position = PlayerController.transform.position;
                position.y += PlayerController.CharacterController.skinWidth;
                PlayerController.transform.position = position;
            }
        }
        
    }
}