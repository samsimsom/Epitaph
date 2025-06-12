using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class JumpHandler : MovementSubBehaviour
    {
        // Jump Variables
        public float JumpForce = 5.0f;
        public float AirControlFactor = 1.25f;

        public JumpHandler(MovementBehaviour movementBehaviour, 
            PlayerController playerController) : base(movementBehaviour, playerController) { }
        
        public void PerformJump()
        {
            // Zıplamadan önce dikey hızı sıfırlayarak birikmiş yerçekimi etkisini kaldır.
            MovementBehaviour.VerticalMovement = 0f;
            
            // Zıplama kuvvetini uygula.
            MovementBehaviour.VerticalMovement += JumpForce;
            MovementBehaviour.IsJumping = true;
        }
        
        public bool CanJump()
        {
            // Zıplayabilmek için coyote time'ın geçerli olması ve
            // karakterin üzerinde bir engel olmaması gerekir.
            return MovementBehaviour.CoyoteTimeCounter > 0f && !HasObstacleAbove();
        }
        
        private bool HasObstacleAbove()
        {
            if (PlayerController.CharacterController == null) return true;

            var capsuleCollider = PlayerController.CharacterController;
            var center = capsuleCollider.bounds.center;
            var radius = capsuleCollider.radius * 0.9f;
            var height = capsuleCollider.height;
            var checkDistance = height * 0.5f + JumpForce * 0.1f;

            var layerMask = ~(1 << PlayerController.gameObject.layer);

            return Physics.CheckCapsule(center, center + Vector3.up * checkDistance,
                radius, layerMask);
        }
        
    }
}