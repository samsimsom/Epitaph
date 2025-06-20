namespace Epitaph.Scripts.Player.BaseBehaviour
{
    public abstract class PlayerBehaviour
    {
        public PlayerController PlayerController;
        
        protected PlayerBehaviour(PlayerController playerController)
        {
            PlayerController = playerController;
        }
        
        public virtual void Awake() { }
        public virtual void OnEnable() { }
        
        public virtual void Start() { }
        
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }
        
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }

        // ---------------------------------------------------------------------------- //
        
        public virtual void OnGUI() { }
        public virtual void OnDrawGizmos() { }
        
    }
}