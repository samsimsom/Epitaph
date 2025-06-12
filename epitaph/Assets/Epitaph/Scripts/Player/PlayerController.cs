using Epitaph.Scripts.InputManager;
using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ViewSystem;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraTransform;
        
        // ---------------------------------------------------------------------------- //
        
        public PlayerInput PlayerInput => playerInput;
        public CharacterController CharacterController => characterController;
        public Camera PlayerCamera => playerCamera;
        public Transform CameraTransform => cameraTransform;
        
        // ---------------------------------------------------------------------------- //
        
        #region Player Behaviours
        
        private PlayerBehaviourManager<PlayerBehaviour> _behaviourManager;
        public ViewBehaviour ViewBehaviour { get; private set; }
        public MovementBehaviour MovementBehaviour { get; private set; }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
        #region Unity Lifecycle Methods

        private void Awake()
        {
            InitializeBehaviours();
            
            _behaviourManager.ExecuteOnAll(b => b.Awake());
        }

        private void OnEnable()
        {
            _behaviourManager.ExecuteOnAll(b => b.OnEnable());
        }

        private void Start()
        {
            _behaviourManager.ExecuteOnAll(b => b.Start());
        }

        private void Update()
        {
            _behaviourManager.ExecuteOnAll(b => b.Update());
        }

        private void LateUpdate()
        {
            _behaviourManager.ExecuteOnAll(b => b.LateUpdate());
        }

        private void FixedUpdate()
        {
            _behaviourManager.ExecuteOnAll(b => b.FixedUpdate());
        }

        private void OnDisable()
        {
            _behaviourManager.ExecuteOnAll(b => b.OnDisable());
        }

        private void OnDestroy()
        {
            _behaviourManager.ExecuteOnAll(b => b.OnDestroy());
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            _behaviourManager?.ExecuteOnAll(b => b?.OnGUI());
        }

        private void OnDrawGizmos()
        {
            _behaviourManager?.ExecuteOnAll(b => b?.OnDrawGizmos());
        }
#endif

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
        #region Initialization
        
        private void InitializeBehaviours()
        {
            _behaviourManager = new PlayerBehaviourManager<PlayerBehaviour>(this);

            ViewBehaviour = _behaviourManager.AddBehaviour(new ViewBehaviour(this));
            MovementBehaviour = _behaviourManager.AddBehaviour(new MovementBehaviour(this));
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}