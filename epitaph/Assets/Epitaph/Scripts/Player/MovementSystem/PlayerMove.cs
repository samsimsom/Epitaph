using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerMove : PlayerBehaviour
    {
        private PlayerData _playerData;
        private CharacterController _characterController;
        private Camera _playerCamera;
        private float _speed;

        public PlayerMove(PlayerController playerController,
            PlayerData playerData,
            CharacterController characterController,
            Camera playerCamera) : base(playerController)
        {
            _playerData = playerData;
            _characterController = characterController;
            _playerCamera = playerCamera;

            Initialize();
        }

        #region MonoBehaviour Methods
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
            ProcessMove(PlayerController.GetPlayerInput().moveInput);
        }
        #endregion

        #region Public Methods
        public void ProcessMove(Vector2 input)
        {
            var direction = CalculateMoveDirection(input);
            var movement = direction * (_speed * Time.deltaTime);
            _characterController.Move(movement);
            _playerData.currentVelocity.x = _characterController.velocity.x;
            _playerData.currentVelocity.z = _characterController.velocity.z;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            _speed = _playerData.walkSpeed;
        }

        private void AdjustPlayerPosition()
        {
            // CharacterControllerdan gelen skinWidth offseti pozisyona ekleniyor.
            var playerHeight = _characterController.skinWidth;
            PlayerController.transform.position += new Vector3(0, playerHeight, 0);
        }

        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            // Kamera referansını al
            var cameraTransform = _playerCamera.transform;

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

        private void OnChangeCrouchSpeed(float obj)
        {
            _speed = obj;
        }

        private void OnChangeSprintSpeed(float obj)
        {
            _speed = obj;
        }
        #endregion
    }
}