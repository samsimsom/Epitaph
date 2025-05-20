using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerMove : PlayerBehaviour
    {
        private PlayerMovementData _playerMovementData;
        private PlayerInput _playerInput;
        private CharacterController _characterController;
        private PlayerLook _playerLook;
        
        private float _speed;
        
        public PlayerMove(PlayerController playerController, 
            PlayerMovementData playerMovementData, 
            PlayerInput playerInput, 
            CharacterController characterController, 
            PlayerLook playerLook) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            _playerInput = playerInput;
            _characterController = characterController;
            _playerLook = playerLook;
            
            Initialize();
        }
        
        #region MonoBehaviour Methots
        public override void OnEnable()
        {
            PlayerSprint.OnChangeSprintSpeed += OnChangeSprintSpeed;
            PlayerCrouch.OnChangeCrouchSpeed += OnChangeCrouchSpeed;
        }

        public override void OnDisable()
        {
            PlayerSprint.OnChangeSprintSpeed -= OnChangeSprintSpeed;
            PlayerCrouch.OnChangeCrouchSpeed -= OnChangeCrouchSpeed;
        }

        public override void Start()
        {
            AdjustPlayerPosition();
        }
        
        public override void Update()
        {
            ProcessMove(_playerInput.moveInput);
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
            _speed = _playerMovementData.walkSpeed;
        }
        
        private void AdjustPlayerPosition()
        {
            // CharacterControllerdan gelen skinWidth offseti pozisyona ekleniyor.
            var playerHeight = _characterController.skinWidth;
            // transform.position += new Vector3(0, playerHeight, 0);
        }
        
        #region Move Methods
        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            // Kamera referansını al
            var cameraTransform = _playerLook.CameraTransform;
    
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
            _characterController.Move(movement);
            _playerMovementData.currentVelocity.x = _characterController.velocity.x;
            _playerMovementData.currentVelocity.z = _characterController.velocity.z;
        }
        #endregion
        
    }
}