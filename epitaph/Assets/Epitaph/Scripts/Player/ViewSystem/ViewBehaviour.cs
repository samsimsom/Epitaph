using System.Collections.Generic;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class ViewBehaviour : PlayerBehaviour
    {
        // Properties
        public Look Look { get; private set; }
        public HeadBob HeadBob { get; private set; }
        
        // Head Bob Configuration
        public float HeadBobAmount = 0.02f;
        public float HeadBobFrequency = 7.5f;
        public float HeadBobSmooth = 10.0f;
        public float HeadBobThreshold = 1.5f;
        
        // Crouch Configuration
        public float CameraTransitonSmooth = 10.0f;
        
        // Camera Position Management
        private Vector3 _basePosition;
        private Vector3 _headBobOffset = Vector3.zero;
        private float _targetHeight;
        private float _currentHeight;
        
        // Random Offset Configuration
        private Vector3 _randomOffset = Vector3.zero;
        
        // Child Behaviours
        private readonly List<PlayerBehaviour> _viewBehaviours = new();
        
        // ---------------------------------------------------------------------------- //

        public ViewBehaviour(PlayerController playerController)
            : base(playerController)
        {
            InitializeCameraBasePosition();
            InitializeBehaviours();
        }

        // ---------------------------------------------------------------------------- //
        
        #region Camera Position Methods
        
        private void InitializeCameraBasePosition()
        {
            _basePosition = PlayerController.CameraTransform.localPosition;
            _currentHeight = _basePosition.y;
            _targetHeight = _currentHeight;
        }

        public void SetHeadBobOffset(Vector3 offset)
        {
            _headBobOffset = offset;
            UpdateCameraPosition();
        }

        public void SetCameraHeight(float height)
        {
            _targetHeight = height;
            UpdateCameraPosition();
        }

        public void UpdateCameraPosition()
        {
            // Smooth height transition
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, 
                Time.deltaTime * CameraTransitonSmooth);
            
            // Combine all effects
            var finalPosition = new Vector3
            {
                x = _basePosition.x + _headBobOffset.x + _randomOffset.x,
                y = _currentHeight + _headBobOffset.y + _randomOffset.y,
                z = _basePosition.z + _headBobOffset.z + _randomOffset.z
            };
            
            PlayerController.CameraTransform.localPosition = finalPosition;
        }

        public void CameraReset()
        {
            _headBobOffset = Vector3.zero;
            _randomOffset = Vector3.zero;
            _targetHeight = _basePosition.y;
            UpdateCameraPosition();
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region MonoBehaviour Methods
        
        public override void Awake()
        {
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
            UpdateCameraPosition();
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
                Look = AddViewBehaviour(new Look(this, PlayerController));
                HeadBob = AddViewBehaviour(new HeadBob(this, PlayerController));
            }
            else
            {
                Debug.LogError("PlayerCameraTransform is null in ViewBehaviour." +
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