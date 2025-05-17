using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerMove : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
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
            PlayerSprint.OnChangeSprintSpeed += speed => { _speed = speed; };
            PlayerCrouch.OnChangeCrouchSpeed += speed => { _speed = speed; };
        }

        private void OnDisable()
        {
            PlayerSprint.OnChangeSprintSpeed -= speed => { _speed = speed; };
            PlayerCrouch.OnChangeCrouchSpeed -= speed => { _speed = speed; };
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
        
        private void Initialize()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            
            _speed = playerData.walkSpeed;
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
            playerData.currentVelocity.x = characterController.velocity.x;
            playerData.currentVelocity.z = characterController.velocity.z;
        }
        #endregion
        
    }
}