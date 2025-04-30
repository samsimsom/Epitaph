using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        public void ProcessMove(Vector2 input)
        {
            var movementDirection = CalculateMovementDirection(input);
            MoveCharacter(movementDirection);
        }
        
        private Vector3 CalculateMovementDirection(Vector2 input)
        {
            // Get camera direction vectors
            var cameraTransform = playerCamera.transform;
            var forward = cameraTransform.forward;
            var right = cameraTransform.right;
            
            // Project to XZ plane (horizontal movement only)
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Calculate direction based on input and camera orientation
            return (forward * input.y + right * input.x);
        }
        
        private void MoveCharacter(Vector3 direction)
        {
            var movement = direction * (moveSpeed * Time.deltaTime);
            characterController.Move(movement);
        }
    }
}