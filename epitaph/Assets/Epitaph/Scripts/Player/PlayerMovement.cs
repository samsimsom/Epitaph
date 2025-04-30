using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Epitaph.Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float speed;

        private void Start()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }

        public void ProcessMove(Vector2 input)
        {
            // Get the camera's forward and right vectors (excluding vertical component)
            var cameraTransform = mainCamera.transform;
            var forward = cameraTransform.forward;
            var right = cameraTransform.right;
            
            // Project these vectors onto the XZ plane and normalize them
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Calculate the direction relative to where the player is looking
            var desiredMoveDirection = (forward * input.y + right * input.x);
            
            // Move in that direction
            characterController.Move(desiredMoveDirection * (speed * Time.deltaTime));
        }
        
    }
}