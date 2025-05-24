using System.Collections.Generic;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class ViewBehaviour : PlayerBehaviour
    {
        public HeadBob HeadBob { get; private set; }
        
        // Movement Variables
        public float HeadBobAmount = 0.02f;
        public float HeadBobFrequency = 10.0f;
        public float HeadBobSmooth = 10.0f;
        public float HeadBobThreshold = 5.0f;
        
        private readonly List<PlayerBehaviour> _viewBehaviours = new();
        
        
        // ---------------------------------------------------------------------------- //
        
        public ViewBehaviour(PlayerController playerController) 
            : base(playerController) { }

        // ---------------------------------------------------------------------------- //

        #region MonoBehaviour Methods
        
        public override void Awake()
        {
            InitializeBehaviours();
            foreach (var behaviour in _viewBehaviours) behaviour.Awake();
        }

        public override void OnEnable()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnEnable();
        }

        public override void Start()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Start();
        }

        public override void Update()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Update();
        }

        public override void LateUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.LateUpdate();
        }

        public override void FixedUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.FixedUpdate();
        }

        public override void OnDisable()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDisable();
        }

        public override void OnDestroy()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDestroy();
        }
        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDrawGizmos();
        }
#endif

        #endregion
        
        // ---------------------------------------------------------------------------- //

        public void InitializeBehaviours()
        {
            if (PlayerController.CameraTransform != null)
            {
                // PlayerLook = AddViewBehaviour(new PlayerLook()); 
                HeadBob = AddViewBehaviour(new HeadBob(this, PlayerController));
            }
            else
            {
                Debug.LogError("PlayerCameraTransform is null in ViewController." +
                               "InitializeBehaviours. View behaviours cannot be " +
                               "initialized.", PlayerController);
            }
        }
        
        private T AddViewBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _viewBehaviours.Add(behaviour);
            return behaviour;
        }
        
    }
}