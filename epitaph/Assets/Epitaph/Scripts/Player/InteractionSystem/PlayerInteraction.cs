using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.Interaction;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.InteractionSystem
{
    public class PlayerInteraction : PlayerBehaviour
    {
        // Events
        public event Action<IInteractable> OnInteractableFound;
        public event Action OnInteractableLost;

        // Private Fields
        private readonly PlayerController _playerController;
        private readonly PlayerData _playerData;
        private readonly Camera _playerCamera;
        
        private RaycastHit _lastHit;
        private bool _didHit;
        private Vector3 _rayDirection;
        private Vector3 _rayOrigin;
        private CancellationTokenSource _cts;
        private IInteractable _currentInteractable;

        // Constructor
        public PlayerInteraction(PlayerController playerController, 
            PlayerData playerData, Camera playerCamera) : base(playerController)
        {
            _playerController = playerController;
            _playerData = playerData;
            _playerCamera = playerCamera;
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

        // Public Methods
        public void TryInteract()
        {
            _currentInteractable?.Interact();
        }
        
        public void ProcessInteraction()
        {
            _currentInteractable?.Interact();
        }

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

        // Private Methods
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
                    _playerData.interactionDistance, _playerData.interactableLayer);

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
                await UniTask.Delay(TimeSpan.FromSeconds(_playerData.raycastInterval), 
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

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (!_playerData.showDebugGizmos) return;
            if (_didHit)
            {
                Gizmos.color = _playerData.hitGizmoColor;
                Gizmos.DrawLine(_rayOrigin, _lastHit.point);
                Gizmos.DrawSphere(_lastHit.point, 0.025f);
            }
            else
            {
                Gizmos.color = _playerData.gizmoColor;
                Gizmos.DrawLine(_rayOrigin, _rayOrigin + _rayDirection * _playerData.interactionDistance);
                Gizmos.DrawSphere(_rayOrigin + _rayDirection * _playerData.interactionDistance, 0.025f);
            }
        }
#endif
        
    }
}