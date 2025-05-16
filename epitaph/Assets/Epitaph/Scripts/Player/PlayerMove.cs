using System;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerMove : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        #region MonoBehaviour Methots
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            AdjustPlayerPosition();
        }
        
        private void Update()
        {
            ProcessMove(playerInput.moveInput);
        }
        #endregion
        
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
        
        private void AdjustPlayerPosition()
        {
            // CharacterControllerdan gelen skinWidth offseti pozisyona ekleniyor.
            var playerHeight = characterController.skinWidth;
            transform.position += new Vector3(0, playerHeight, 0);
        }
        
        #region Move Methods
        private Vector3 CalculateMoveDirection(Vector2 input)
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

        public void ProcessMove(Vector2 input)
        {
            var direction = CalculateMoveDirection(input);
            var movement = direction * (moveSpeed * Time.deltaTime);
            characterController.Move(movement);
        }
        #endregion

        // private void OnControllerColliderHit(ControllerColliderHit hit)
        // {
        //     Debug.Log(hit.gameObject.name);
        // }

        #region Public Methods
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public float GetMoveSpeed()
        {
            return moveSpeed;
        }
        #endregion
        
    }
}