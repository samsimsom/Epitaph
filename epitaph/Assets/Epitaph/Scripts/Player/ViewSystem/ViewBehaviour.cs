using System.Collections.Generic;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class ViewBehaviour : PlayerBehaviour
    {
        // Properties
        public HeadBob HeadBob { get; private set; }
        
        // Head Bob Configuration
        public float HeadBobAmount = 0.02f;
        public float HeadBobFrequency = 10.0f;
        public float HeadBobSmooth = 10.0f;
        public float HeadBobThreshold = 1.5f;
        
        // Camera Position Management
        private Vector3 _basePosition;
        private Vector3 _headBobOffset = Vector3.zero;
        private float _targetHeight;
        private float _currentHeight;
        
        // Child Behaviours
        private readonly List<PlayerBehaviour> _viewBehaviours = new();
        
        // ---------------------------------------------------------------------------- //

        public ViewBehaviour(PlayerController playerController)
            : base(playerController)
        {
            InitializeBasePositionAndHeight();
        }

        private void InitializeBasePositionAndHeight()
        {
            _basePosition = PlayerController.CameraTransform.localPosition;
            _currentHeight = _basePosition.y;
            _targetHeight = _currentHeight;
        }

        // ---------------------------------------------------------------------------- //
        
        #region Camera Position Methods

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
                Time.deltaTime * 10.0f);
            
            // Combine all effects
            var finalPosition = new Vector3
            {
                x = _basePosition.x + _headBobOffset.x,
                y = _currentHeight + _headBobOffset.y,
                z = _basePosition.z + _headBobOffset.z
            };
            
            PlayerController.CameraTransform.localPosition = finalPosition;
        }

        public void CameraReset()
        {
            _headBobOffset = Vector3.zero;
            _targetHeight = _basePosition.y;
            UpdateCameraPosition();
        }

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region MonoBehaviour Methods
        
        public override void Awake()
        {
            InitializeBehaviours();
            ForEachBehaviour(behaviour => behaviour.Awake());
        }

        public override void OnEnable()
        {
            ForEachBehaviour(behaviour => behaviour.OnEnable());
        }

        public override void Start()
        {
            ForEachBehaviour(behaviour => behaviour.Start());
        }

        public override void Update()
        {
            ForEachBehaviour(behaviour => behaviour.Update());
        }

        public override void LateUpdate()
        {
            ForEachBehaviour(behaviour => behaviour.LateUpdate());
            
            // Update camera position at the end of each frame
            UpdateCameraPosition();
        }

        public override void FixedUpdate()
        {
            ForEachBehaviour(behaviour => behaviour.FixedUpdate());
        }

        public override void OnDisable()
        {
            ForEachBehaviour(behaviour => behaviour.OnDisable());
        }

        public override void OnDestroy()
        {
            ForEachBehaviour(behaviour => behaviour.OnDestroy());
        }
        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            ForEachBehaviour(behaviour => behaviour.OnDrawGizmos());
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
        
        private void ForEachBehaviour(System.Action<PlayerBehaviour> action)
        {
            foreach (var behaviour in _viewBehaviours)
            {
                action(behaviour);
            }
        }
    }
}