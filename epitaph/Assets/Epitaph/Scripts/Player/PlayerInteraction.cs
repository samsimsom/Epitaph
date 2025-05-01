// ReSharper disable CommentTypo, IdentifierTypo
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        
        [Header("Raycast Settings")]
        public float interactionDistance = 3f;
        public LayerMask interactableLayer;

        [Header("Gizmo Settings")]
        public Color hitGizmoColor = Color.yellow;
        public Color gizmoColor = Color.red;

        private void Update()
        {
            // Send a ray from the center of the camera's view
            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            // Check if the ray hits something on the interactable layer within the
            // interaction distance
            if (Physics.Raycast(ray, out var hit, interactionDistance, interactableLayer))
            {
                // You can add your interaction logic here
                // For example, you can call a method on the hit object:
                // Interactable interactable = hit.collider.GetComponent<Interactable>();
                // if (interactable != null)
                // {
                //     interactable.Interact();
                // }

                if (hit.collider != null)
                {
                    // Debug.Log("Hit: " + hit.collider.name);
                }

                // Draw a green line to the hit point
                Debug.DrawLine(ray.origin, hit.point, hitGizmoColor);
            }
            else
            {
                // Draw a red line for the full interaction distance
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * 
                    interactionDistance, gizmoColor);
            }
        }
    }
}