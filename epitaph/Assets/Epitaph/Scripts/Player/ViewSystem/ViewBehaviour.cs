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
        public float HeadBobThreshold = 1.5f;
        
        private Vector3 _basePosition;
        private Vector3 _headBobOffset = Vector3.zero;
        
        private float _targetHeight;
        private float _currentHeight;
        
        private readonly List<PlayerBehaviour> _viewBehaviours = new();
        
        // ---------------------------------------------------------------------------- //

        public ViewBehaviour(PlayerController playerController)
            : base(playerController)
        {
            _basePosition = playerController.CameraTransform.localPosition;
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
            // Yumuşak yükseklik geçişi
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, Time.deltaTime * 10.0f);
            
            // Tüm etkileri birleştir
            var finalPosition = new Vector3()
            {
                x = _basePosition.x + _headBobOffset.x,
                y = _currentHeight + _headBobOffset.y,
                z = _basePosition.z + _headBobOffset.z
            };
            
            PlayerController.CameraTransform.localPosition = finalPosition;
        }

        public void Reset()
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
            
            // Her frame sonunda kamera pozisyonunu güncelle
            UpdateCameraPosition();
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