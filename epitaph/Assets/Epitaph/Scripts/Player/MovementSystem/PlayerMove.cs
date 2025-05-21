using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerMove : PlayerBehaviour
    {
        private PlayerData _playerData;
        private CharacterController _characterController;
        private Camera _playerCamera;

        public float Speed { get; private set; } = 1f;
        public float MaxSpeed { get; private set; } = 20f;
        public float Modifier { get; set; } = 1f;
        public float EffectiveSpeed => Speed * Modifier;

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
            var speed = Mathf.Clamp(Speed, EffectiveSpeed, MaxSpeed);
            var movement = direction * (speed * Time.deltaTime);
            _characterController.Move(movement);

            // DEBUG
            _playerData.currentSpeed = speed;
            _playerData.currentVelocity.x = _characterController.velocity.x;
            _playerData.currentVelocity.z = _characterController.velocity.z;
        }
        
        public void SetWalkingSpeed()
        {
            Modifier = 2.75f;
        }
        
        public void SetRunningSpeed()
        {
            Modifier = 4.75f;
        }
        
        public void SetCrouchingSpeed()
        {
            Modifier = 1.5f;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            SetWalkingSpeed();
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
        #endregion
    }
}