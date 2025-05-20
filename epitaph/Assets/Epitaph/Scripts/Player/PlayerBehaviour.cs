namespace Epitaph.Scripts.Player
{
    public abstract class PlayerBehaviour
    {
        protected PlayerController PlayerController;
        protected PlayerBehaviour(PlayerController playerController)
        {
            PlayerController = playerController;
        }

        public abstract void Start();
        public abstract void OnEnable();
        public abstract void OnDisable();
        public abstract void Update();

#if UNITY_EDITOR
        public abstract void OnDrawGizmos();
#endif
    }
}