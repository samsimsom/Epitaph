using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.Interaction;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerInteraction : PlayerBehaviour
    {
     
        public PlayerInteraction(PlayerController playerController, 
            Camera playerCamera) : base(playerController)
        {
            _playerCamera = playerCamera;
        }
        
        [Header("Raycast Settings")]
        public float InteractionDistance = 3f;
        public LayerMask InteractableLayer = LayerMask.GetMask("Interactable");
        private float _raycastInterval = 0.05f;
        
        [Header("Debug Settings")]
        public bool ShowDebugGizmos = true;
        public Color HitGizmoColor = Color.yellow;
        public Color GizmoColor = Color.red;

        private Camera _playerCamera;
        
        // Events
        public event Action<IInteractable> OnInteractableFound;
        public event Action OnInteractableLost;

        private RaycastHit _lastHit;
        private bool _didHit;
        private Vector3 _rayDirection;
        private Vector3 _rayOrigin;
        private CancellationTokenSource _cts;
        private IInteractable _currentInteractable;

        public override void Start()
        {
            
        }

        public override void OnEnable()
        {
            _cts = new CancellationTokenSource();
            StartRaycastLoop(_cts.Token).Forget();
        }

        public override void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid StartRaycastLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Calculate ray parameters
                _rayOrigin = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).origin;
                _rayDirection = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;

                // Perform raycast
                var previousHitState = _didHit; 
                _didHit = Physics.Raycast(_rayOrigin, _rayDirection, out _lastHit, 
                    InteractionDistance, InteractableLayer);

                switch (_didHit)
                {
                    // Check if we found or lost an interactable
                    case true when (!previousHitState || _currentInteractable == null):
                        CheckForInteractable();
                        break;
                    case false when previousHitState && _currentInteractable != null:
                        _currentInteractable = null;
                        OnInteractableLost?.Invoke();
                        break;
                }
                
                // Run at intervals to reduce CPU usage
                await UniTask.Delay(TimeSpan.FromSeconds(_raycastInterval), 
                    cancellationToken: cancellationToken);
            }
        }

        private void CheckForInteractable()
        {
            if (!_didHit || _lastHit.collider == null) return;
            
            var interactable = _lastHit.collider.GetComponent<IInteractable>();
            
            if (interactable == null) return;
            _currentInteractable = interactable;
            OnInteractableFound?.Invoke(interactable);
        }

        public override void Update()
        {
            DebugDrawLine();
        }

        private void DebugDrawLine()
        {
            if (!ShowDebugGizmos) return;
            
            // Debug visualization
            if (_didHit)
            {
                Debug.DrawLine(_rayOrigin, _lastHit.point, HitGizmoColor);
            }
            else
            {
                Debug.DrawLine(_rayOrigin, _rayOrigin + _rayDirection * InteractionDistance, GizmoColor);
            }
        }

        #region Public Methods
        // Public method to handle interaction input
        public void TryInteract()
        {
            _currentInteractable?.Interact();
        }
        
        public void ProcessInteraction()
        {
            _currentInteractable?.Interact();
        }

        // Helper methods
        public bool IsTargetingInteractable()
        {
            return _didHit && _currentInteractable != null;
        }

        public GameObject GetTargetedObject()
        {
            return _didHit ? _lastHit.collider.gameObject : null;
        }

        public RaycastHit GetLastHit()
        {
            return _lastHit;
        }
        
        public IInteractable GetCurrentInteractable()
        {
            return _currentInteractable;
        }
        #endregion
    }
}