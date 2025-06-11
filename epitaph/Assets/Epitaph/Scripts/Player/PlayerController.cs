using System.Collections.Generic;
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
            
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.Awake());
        }

        private void OnEnable()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.OnEnable());
        }

        private void Start()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.Start());
        }

        private void Update()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.Update());
        }

        private void LateUpdate()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.LateUpdate());
        }

        private void FixedUpdate()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.FixedUpdate());
        }

        private void OnDisable()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.OnDisable());
        }

        private void OnDestroy()
        {
            _behaviourManager.ExecuteOnAll(behaviour => behaviour.OnDestroy());
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            _behaviourManager?.ExecuteOnAll(behaviour => behaviour?.OnGUI());
        }

        private void OnDrawGizmos()
        {
            _behaviourManager?.ExecuteOnAll(behaviour => behaviour?.OnDrawGizmos());
        }
#endif

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
        #region Initialization

        // private T AddBehaviour<T>(T behaviour) where T : PlayerBehaviour
        // {
        //     _playerBehaviours.Add(behaviour);
        //     return behaviour;
        // }

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