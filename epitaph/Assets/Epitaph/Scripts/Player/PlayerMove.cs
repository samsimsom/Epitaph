using System;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerMove : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerLook playerLook;

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
            // Kamera referansını al
            var cameraTransform = playerLook.CameraTransform;
    
            // Kameranın ileri ve sağ vektörlerini al
            var forward = cameraTransform.forward;
            var right = cameraTransform.right;
    
            // Y bileşenini sıfırla (yatay düzlemde hareket için)
            forward.y = 0;
            right.y = 0;
    
            // Vektörleri normalize et
            forward.Normalize();
            right.Normalize();
    
            // İleri/geri hareketi için forward vektörünü, sağ/sol hareketi
            // için right vektörünü kullan
            // ve bunları input değerlerine göre ölçeklendir
            var moveDirection = (forward * input.y) + (right * input.x);
    
            return moveDirection;
        }

        public void ProcessMove(Vector2 input)
        {
            var direction = CalculateMoveDirection(input);
            var movement = direction * (moveSpeed * Time.deltaTime);
            characterController.Move(movement);
        }
        #endregion
        
    }
}