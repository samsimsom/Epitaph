namespace Epitaph.Scripts.Player
{
    public abstract class PlayerBehaviour : IBehaviour
    {
        protected PlayerController PlayerController;
        protected PlayerBehaviour(PlayerController playerController)
        {
            PlayerController = playerController;
        }

        public virtual void Awake() { /* Varsayılan boş implementasyon */ }
        public virtual void OnEnable()  { /* Varsayılan boş implementasyon */ }
        public virtual void Start() { /* Varsayılan boş implementasyon */ }
        public virtual void Update() { /* Varsayılan boş implementasyon */ }
        public virtual void LateUpdate() { /* Varsayılan boş implementasyon */ }
        public virtual void FixedUpdate() { /* Varsayılan boş implementasyon */ }
        public virtual void OnDisable() { /* Varsayılan boş implementasyon */ }
        public virtual void OnDestroy() { /* Varsayılan boş implementasyon */ }
        
#if UNITY_EDITOR
        public virtual void OnDrawGizmos() { /* Varsayılan boş implementasyon */ }
#endif
    }
}