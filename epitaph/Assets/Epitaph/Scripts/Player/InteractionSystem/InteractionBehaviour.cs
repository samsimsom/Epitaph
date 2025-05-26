using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.Interactables;
using UnityEngine;
using Epitaph.Scripts.Player.BaseBehaviour;

namespace Epitaph.Scripts.Player.InteractionSystem
{
    public class InteractionBehaviour : PlayerBehaviour
    {
        // Events
        public event Action<IInteractable> OnInteractableFound;
        public event Action OnInteractableLost;

        private RaycastHit _lastHit;
        private bool _didHit;
        private Vector3 _rayDirection;
        private Vector3 _rayOrigin;
        private CancellationTokenSource _cts;
        private IInteractable _currentInteractable;

        // Public fields for Inspector configuration
        // Consider making these [SerializeField] private fields for better encapsulation
        // if they don't need to be accessed from other scripts.
        public float RaycastInterval = 0.05f;
        public float InteractionDistance = 10.0f;
        public bool ShowDebugGizmos = true;
        public LayerMask InteractableLayer = LayerMask.GetMask("Interactable");

        public InteractionBehaviour(PlayerController playerController)
            : base(playerController) { }

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

        public bool IsTargetingInteractable()
        {
            // With the refined raycast loop, _currentInteractable will accurately reflect
            // if we are targeting a valid interactable.
            return _currentInteractable != null;
        }

        public GameObject GetTargetedObject()
        {
            // _didHit ensures _lastHit is relevant.
            return _didHit && _lastHit.collider != null ? _lastHit.collider.gameObject : null;
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
                // Calculate ray parameters once
                Camera playerCamera = PlayerController.PlayerCamera;
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                _rayOrigin = ray.origin;
                _rayDirection = ray.direction;

                IInteractable previouslyFocusedInteractable = _currentInteractable;

                _didHit = Physics.Raycast(_rayOrigin, _rayDirection, out _lastHit,
                    InteractionDistance, InteractableLayer);

                IInteractable newlyFoundInteractable = null;
                if (_didHit && _lastHit.collider != null)
                {
                    newlyFoundInteractable = _lastHit.collider.GetComponent<IInteractable>();
                }

                // Check if the focused interactable has changed
                if (newlyFoundInteractable != previouslyFocusedInteractable)
                {
                    // If we were focused on an interactable and it's no longer the focus
                    if (previouslyFocusedInteractable != null)
                    {
                        OnInteractableLost?.Invoke();
                    }

                    _currentInteractable = newlyFoundInteractable; // Update to the new interactable (can be null)

                    // If the new focus is a valid interactable
                    if (_currentInteractable != null)
                    {
                        OnInteractableFound?.Invoke(_currentInteractable);
                    }
                }
                // If newlyFoundInteractable == previouslyFocusedInteractable, no change, so no events are fired.

                await UniTask.Delay(TimeSpan.FromSeconds(RaycastInterval),
                    cancellationToken: cancellationToken);
            }
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (!ShowDebugGizmos || PlayerController?.PlayerCamera == null) return;

            // Ensure _rayOrigin and _rayDirection are initialized if called before first raycast
            // or if StartRaycastLoop hasn't run yet (e.g. component disabled then enabled)
            // For robustness, we can re-calculate them here if needed, or rely on them being valid.
            // Assuming they are valid from the loop for drawing the most recent ray.

            if (Application.isPlaying) // Only draw if the loop is potentially running and values are set
            {
                Gizmos.color = _didHit && _currentInteractable != null ? Color.green : Color.red;
                if (_didHit)
                {
                    Gizmos.DrawLine(_rayOrigin, _lastHit.point);
                    Gizmos.DrawSphere(_lastHit.point, 0.025f);
                }
                else
                {
                    Gizmos.DrawLine(_rayOrigin, _rayOrigin + _rayDirection * InteractionDistance);
                    Gizmos.DrawSphere(_rayOrigin + _rayDirection * InteractionDistance, 0.025f);
                }
            }
            else // Draw a representation from the camera if not playing
            {
                Camera playerCamera = PlayerController.PlayerCamera;
                Ray editorRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(editorRay.origin, editorRay.origin + editorRay.direction * InteractionDistance);
            }
        }
#endif
    }
}