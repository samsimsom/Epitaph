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