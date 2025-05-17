using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerMove : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("References")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerLook playerLook;
        
        private float _speed;

        #region MonoBehaviour Methots
        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            PlayerSprint.OnChangeSprintSpeed += OnChangeSprintSpeed;
            PlayerCrouch.OnChangeCrouchSpeed += OnChangeCrouchSpeed;
        }

        private void OnDisable()
        {
            PlayerSprint.OnChangeSprintSpeed -= OnChangeSprintSpeed;
            PlayerCrouch.OnChangeCrouchSpeed -= OnChangeCrouchSpeed;
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
        
        private void OnChangeCrouchSpeed(float obj)
        {
            _speed = obj;
        }

        private void OnChangeSprintSpeed(float obj)
        {
            _speed = obj;
        }
        
        private void Initialize()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            
            _speed = playerMovementData.walkSpeed;
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
            var movement = direction * (_speed * Time.deltaTime);
            characterController.Move(movement);
            playerMovementData.currentVelocity.x = characterController.velocity.x;
            playerMovementData.currentVelocity.z = characterController.velocity.z;
        }
        #endregion
        
    }
}