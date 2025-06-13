using System;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // Alt behaviour'lar için manager
        private readonly PlayerBehaviourManager<MovementSubBehaviour> _movementBehaviourManager;

        #region Sub behaviours

        public StateManager StateManager { get; private set; }
        public StepHandler StepHandler { get; private set; }
        public GroundHandler GroundHandler { get; private set; }
        public GravityHandler GravityHandler { get; private set; }
        public JumpHandler JumpHandler { get; private set; }
        public FallHandler FallHandler { get; private set; }
        public CrouchHandler CrouchHandler { get; private set; }
        public LocomotionHandler LocomotionHandler { get; private set; }
        public CoyoteTimeHandler CoyoteTimeHandler { get; private set; }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        public MovementBehaviour(PlayerController playerController)
            : base(playerController)
        {
            _movementBehaviourManager = new PlayerBehaviourManager<MovementSubBehaviour>(playerController);
            InitializeBehaviours();
        }
        
        // ---------------------------------------------------------------------------- //
        
        private void InitializeBehaviours()
        {
            StateManager = CreateAndAddBehaviour<StateManager>();
            StepHandler = CreateAndAddBehaviour<StepHandler>();
            GroundHandler = CreateAndAddBehaviour<GroundHandler>();
            GravityHandler = CreateAndAddBehaviour<GravityHandler>();
            JumpHandler = CreateAndAddBehaviour<JumpHandler>();
            FallHandler = CreateAndAddBehaviour<FallHandler>();
            LocomotionHandler = CreateAndAddBehaviour<LocomotionHandler>();
            CrouchHandler = CreateAndAddBehaviour<CrouchHandler>();
            CoyoteTimeHandler = CreateAndAddBehaviour<CoyoteTimeHandler>();
        }
        
        private T CreateAndAddBehaviour<T>() where T : MovementSubBehaviour
        {
            var behaviour = (T)Activator.CreateInstance(typeof(T), this, PlayerController);
            return _movementBehaviourManager.AddBehaviour(behaviour);
        }
        
        // ---------------------------------------------------------------------------- //
        
        #region Unity Lifecycle Methods
        
        public override void Awake()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.Awake());
        }

        public override void OnEnable()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnEnable());
        }

        public override void Start()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.Start());
        }

        public override void Update()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.Update());
        }

        public override void LateUpdate()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.LateUpdate());
        }

        public override void FixedUpdate()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.FixedUpdate());
        }

        public override void OnDisable()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnDisable());
        }

        public override void OnDestroy()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnDestroy());
        }

#if UNITY_EDITOR
        public override void OnGUI()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnGUI());
        }

        public override void OnDrawGizmos()
        {
            DrawCharacterControllerGizmos();
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnDrawGizmos());
        }
#endif

        #endregion

        // ---------------------------------------------------------------------------- //

        #region Unity Editor Methods

        private void DrawCharacterControllerGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 1.0f);
            var center = PlayerController.CharacterController.transform.position + PlayerController.CharacterController.center;
            var height = PlayerController.CharacterController.height;
            var radius = PlayerController.CharacterController.radius;
            var cylinderHeight = Mathf.Max(0, height / 2f - radius);
            var up = PlayerController.CharacterController.transform.up;
            var top = center + up * cylinderHeight;
            var bottom = center - up * cylinderHeight;
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawLine(top + PlayerController.CharacterController.transform.right * radius, bottom + PlayerController.CharacterController.transform.right * radius);
            Gizmos.DrawLine(top - PlayerController.CharacterController.transform.right * radius, bottom - PlayerController.CharacterController.transform.right * radius);
            Gizmos.DrawLine(top + PlayerController.CharacterController.transform.forward * radius, bottom + PlayerController.CharacterController.transform.forward * radius);
            Gizmos.DrawLine(top - PlayerController.CharacterController.transform.forward * radius, bottom - PlayerController.CharacterController.transform.forward * radius);
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}