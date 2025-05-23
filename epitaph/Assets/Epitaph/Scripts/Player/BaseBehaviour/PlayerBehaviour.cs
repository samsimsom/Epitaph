namespace Epitaph.Scripts.Player.BaseBehaviour
{
    public abstract class PlayerBehaviour
    {
        protected PlayerController PlayerController;
        
        protected PlayerBehaviour(PlayerController controller)
        {
            PlayerController = controller;
        }
        
        public virtual void Awake() { }
        public virtual void OnEnable() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }

        public virtual void OnDrawGizmos() { }
        
    }
}