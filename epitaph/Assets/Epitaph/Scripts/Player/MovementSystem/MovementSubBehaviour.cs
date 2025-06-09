using Epitaph.Scripts.Player.BaseBehaviour;

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
        
        public override void Awake() { }
        public override void OnEnable() { }
        public override void Start() { }
        public override void Update() { }
        public override void LateUpdate() { }
        public override void FixedUpdate() { }
        public override void OnDisable() { }
        public override void OnDestroy() { }
        public override void OnGUI() { }
        public override void OnDrawGizmos() { }

    }
}