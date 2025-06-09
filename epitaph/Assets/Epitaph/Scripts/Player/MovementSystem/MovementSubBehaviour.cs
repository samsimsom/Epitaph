using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public abstract class MovementSubBehaviour : PlayerBehaviour
    {
        protected MovementBehaviour MovementBehaviour { get; }

        protected MovementSubBehaviour(MovementBehaviour movementBehaviour, PlayerController playerController)
            : base(playerController)
        {
            MovementBehaviour = movementBehaviour;
        }
    }
}