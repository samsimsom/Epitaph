using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class CoyoteTimeHandler : MovementSubBehaviour
    {
        // Coyote Time Variables
        public float CoyoteTime = 0.2f;
        public float CoyoteTimeCounter { get; private set; }

        public CoyoteTimeHandler(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController)
        {
        }

        public override void Update()
        {
            ManageCoyoteTime();
        }

        private void ManageCoyoteTime()
        {
            if (MovementBehaviour.GroundHandler.IsGrounded)
            {
                CoyoteTimeCounter = CoyoteTime;
            }
            else
            {
                CoyoteTimeCounter -= Time.deltaTime;
            }
        }
    }
}