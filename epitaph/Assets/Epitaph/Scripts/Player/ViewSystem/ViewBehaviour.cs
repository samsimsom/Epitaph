using System.Collections.Generic;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class ViewBehaviour : PlayerBehaviour
    {
        // Alt behaviour'lar için manager
        private readonly PlayerBehaviourManager<ViewSubBehaviour> _viewBehaviourManager;
        
        // Properties - artık manager üzerinden erişilecek
        public Look Look { get; private set; }
        public HeadBob HeadBob { get; private set; }
        // public CameraShake CameraShake { get; private set; } // Yeni eklenebilecek
        
        // Head Bob Configuration
        public float HeadBobAmount = 0.2f;
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

        public ViewBehaviour(PlayerController playerController) : base(playerController)
        {
            // Manager'ı önce başlat
            _viewBehaviourManager = new PlayerBehaviourManager<ViewSubBehaviour>(playerController);
            
            InitializeCameraBasePosition();
            InitializeBehaviours();
        }

        // ---------------------------------------------------------------------------- //
        
        public void InitializeBehaviours()
        {
            if (PlayerController.CameraTransform != null)
            {
                Look = _viewBehaviourManager.AddBehaviour(new Look(this, PlayerController));
                HeadBob = _viewBehaviourManager.AddBehaviour(new HeadBob(this, PlayerController));
                // CameraShake = _viewBehaviourManager.AddBehaviour(new CameraShake(this, PlayerController));
            }
            else
            {
                Debug.LogError("PlayerCameraTransform is null in ViewBehaviour." +
                               "InitializeBehaviours. View behaviours cannot be " +
                               "initialized.", PlayerController);
            }
        }
        
        // ---------------------------------------------------------------------------- //
        
        #region Camera Position Methods
        
        private void InitializeCameraBasePosition()
        {
            if (PlayerController?.CameraTransform != null)
            {
                _basePosition = PlayerController.CameraTransform.localPosition;
                _currentHeight = _basePosition.y;
                _targetHeight = _currentHeight;
            }
            else
            {
                Debug.LogWarning("CameraTransform is null during InitializeCameraBasePosition", PlayerController);
                _basePosition = Vector3.zero;
                _currentHeight = 0f;
                _targetHeight = 0f;
            }
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
            if (PlayerController?.CameraTransform == null) return;
            
            // Smooth height transition
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, Time.deltaTime * CameraTransitonSmooth);
            
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
            _viewBehaviourManager?.ExecuteOnAll(b => b.Awake());
        }

        public override void OnEnable()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.OnEnable());
        }

        public override void Start()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.Start());
        }

        public override void Update()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.Update());
        }

        public override void LateUpdate()
        {
            UpdateCameraPosition();
            _viewBehaviourManager?.ExecuteOnAll(b => b.LateUpdate());
        }

        public override void FixedUpdate()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.FixedUpdate());
        }

        public override void OnDisable()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.OnDisable());
        }

        public override void OnDestroy()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.OnDestroy());
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.OnDrawGizmos());
        }

        public override void OnGUI()
        {
            _viewBehaviourManager?.ExecuteOnAll(b => b.OnGUI());
        }
#endif

        #endregion
        
    }
}