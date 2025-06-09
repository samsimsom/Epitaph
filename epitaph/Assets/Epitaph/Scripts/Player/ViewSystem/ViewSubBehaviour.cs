using Epitaph.Scripts.Player.BaseBehaviour;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public abstract class ViewSubBehaviour : PlayerBehaviour
    {
        protected ViewBehaviour ViewBehaviour { get; }

        protected ViewSubBehaviour(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(playerController)
        {
            ViewBehaviour = viewBehaviour;
        }
    }
}