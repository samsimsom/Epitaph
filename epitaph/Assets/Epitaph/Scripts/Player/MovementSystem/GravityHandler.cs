using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class GravityHandler : MovementSubBehaviour
    {
        // Gravity Variables
        public float Gravity = 25.0f;
        public float VerticalMovementLimit = -10.0f;
        
        public GravityHandler(MovementBehaviour movementBehaviour, 
            PlayerController playerController) : base(movementBehaviour, playerController) { }
        
        public override void Update()
        {
            CheckIsFalling();
        }

        public override void FixedUpdate()
        {
            HandleGravity();
        }
        
        private void HandleGravity()
        {
            if (!MovementBehaviour.IsGrounded && 
                MovementBehaviour.VerticalMovement > VerticalMovementLimit)
            {
                MovementBehaviour.VerticalMovement -= Gravity * Time.fixedDeltaTime;
            }
        }

        private void CheckIsFalling()
        {
            MovementBehaviour.IsFalling = !MovementBehaviour.GroundHandler.IsGrounded && 
                                          MovementBehaviour.VerticalMovement < 0;
        }
        
    }
}