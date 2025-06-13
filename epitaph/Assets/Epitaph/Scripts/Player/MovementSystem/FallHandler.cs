namespace Epitaph.Scripts.Player.MovementSystem
{
    public class FallHandler : MovementSubBehaviour
    {
        public bool IsFalling { get; set; }
        
        public FallHandler(MovementBehaviour movementBehaviour, 
            PlayerController playerController) : base(movementBehaviour, playerController) { }
    }
}