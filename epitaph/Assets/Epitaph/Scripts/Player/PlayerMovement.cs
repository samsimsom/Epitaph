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

        private void Start()
        {
            var playerHeight = characterController.height / 2f + characterController.skinWidth;
            transform.position = new Vector3(0, playerHeight, 0);
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
            var cameraTransform = playerCamera.transform;
            var forward = cameraTransform.forward;
            var right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            return (forward * input.y + right * input.x);
        }
        
        private void MoveCharacter(Vector3 direction)
        {
            var movement = direction * (moveSpeed * Time.deltaTime);
            characterController.Move(movement);
        }

        // Dışarıdan değişken değiştirme methodları
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public float GetMoveSpeed()
        {
            return moveSpeed;
        }

        public void SetPlayerCamera(Camera cam)
        {
            playerCamera = cam;
        }

        public Camera GetPlayerCamera()
        {
            return playerCamera;
        }

        public void SetCharacterController(CharacterController controller)
        {
            characterController = controller;
        }

        public CharacterController GetCharacterController()
        {
            return characterController;
        }
    }
}