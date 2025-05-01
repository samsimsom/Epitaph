// ReSharper disable CommentTypo, IdentifierTypo
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;
using Epitaph.Scripts.Interaction;

namespace Epitaph.Scripts.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        
        [Header("Raycast Settings")]
        public float interactionDistance = 3f;
        public LayerMask interactableLayer;
        [SerializeField] private float raycastInterval = 0.05f;
        
        [Header("Debug Settings")]
        public bool showDebugGizmos = true;
        public Color hitGizmoColor = Color.yellow;
        public Color gizmoColor = Color.red;

        // Events
        public event Action<IInteractable> OnInteractableFound;
        public event Action OnInteractableLost;

        private RaycastHit _lastHit;
        private bool _didHit;
        private Vector3 _rayDirection;
        private Vector3 _rayOrigin;
        private CancellationTokenSource _cts;
        private IInteractable _currentInteractable;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            StartRaycastLoop(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDestroy()
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
                _rayOrigin = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).origin;
                _rayDirection = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;

                // Perform raycast
                var previousHitState = _didHit; 
                _didHit = Physics.Raycast(_rayOrigin, _rayDirection, out _lastHit, 
                    interactionDistance, interactableLayer);

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
                await UniTask.Delay(TimeSpan.FromSeconds(raycastInterval), 
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

        private void Update()
        {
            if (!showDebugGizmos) return;
            
            // Debug visualization
            if (_didHit)
            {
                Debug.DrawLine(_rayOrigin, _lastHit.point, hitGizmoColor);
            }
            else
            {
                Debug.DrawLine(_rayOrigin, _rayOrigin + _rayDirection * interactionDistance, gizmoColor);
            }
        }

        #region Public Methods
        // Public method to handle interaction input
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