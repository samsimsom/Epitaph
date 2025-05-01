// ReSharper disable CommentTypo, IdentifierTypo
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerMove : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform playerBody;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float movingRotationSpeed = 10f; // Hareket ederken dönüş hızı
        [SerializeField] private float idleRotationSpeed = 3f;    // Dururken dönüş hızı
        [SerializeField] private float snapAngle = 45f;           // Snaplenecek açı (derece)
        [SerializeField] private float snapThreshold = 0.1f;      // Hareket eşiği (karakterin durduğunu anlamak için)
        
        [Header("State (ReadOnly)")]
        [SerializeField] private Vector3 movement;
        
        private float _lastCameraAngle;
        private float _targetRotation;
        private bool _isRotationSnapping;
        
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

        private void Start()
        {
            // CharacterControllerdan gelen skinWidth offseti pozisyona ekleniyor.
            var playerHeight = characterController.skinWidth;
            transform.position += new Vector3(0, playerHeight, 0);
            
            // Init playerCamera angle for body rotation.
            _lastCameraAngle = GetCameraYAngle();
        }

        private void Update()
        {
            HandleRotation();
        }
        
        public void ProcessMove(Vector2 input)
        {
            var movementDirection = CalculateMovementDirection(input);
            MoveCharacter(movementDirection);
            RotateCharacter(movementDirection);
        }

        #region Move Methods
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
            movement = direction * (moveSpeed * Time.deltaTime);
            characterController.Move(movement);
        }
        #endregion

        #region Body Rotation Methods
        private void RotateCharacter(Vector3 movementDirection)
        {
            // Karakter hareket ediyorsa hemen döndür
            if (movementDirection.magnitude > snapThreshold)
            {
                _isRotationSnapping = false;
                RotateTowardsCameraDirection();
            }
            else
            {
                // Karakter duruyorsa kamera açısını kontrol et
                CheckForCameraAngleSnap();
            }
        }

        private void RotateTowardsCameraDirection()
        {
            // Kameranın baktığı yöne göre dönüş açısını hesapla
            var cameraAngle = GetCameraYAngle();
            _targetRotation = cameraAngle;
            
            // Hızlı dönüş
            var currentAngle = Mathf.LerpAngle(playerBody.eulerAngles.y, cameraAngle, 
                movingRotationSpeed * Time.deltaTime);
            playerBody.rotation = Quaternion.Euler(0, currentAngle, 0);
        }
        
        private void CheckForCameraAngleSnap()
        {
            var currentCameraAngle = GetCameraYAngle();
            
            // Kamera açısı değişimini kontrol et
            var angleDifference = Mathf.Abs(Mathf.DeltaAngle(_lastCameraAngle, 
                currentCameraAngle));
            
            // Kamera 45 derecelik bir açı değişimi yaptıysa
            if (angleDifference >= snapAngle && !_isRotationSnapping)
            {
                // En yakın 45 derecelik açıya snap
                _targetRotation = Mathf.Round(currentCameraAngle / snapAngle) * snapAngle;
                _isRotationSnapping = true;
            }
            
            _lastCameraAngle = currentCameraAngle;
        }
        
        private void HandleRotation()
        {
            if (!_isRotationSnapping) return;
            
            // Yavaşça hedef açıya dön
            var currentAngle = Mathf.LerpAngle(playerBody.eulerAngles.y, _targetRotation,
                idleRotationSpeed * Time.deltaTime);
            playerBody.rotation = Quaternion.Euler(0, currentAngle, 0);
                
            // Dönüş tamamlandı mı kontrol et
            if (!(Mathf.Abs(Mathf.DeltaAngle(playerBody.eulerAngles.y, _targetRotation)) <
                  0.5f)) return;
            _isRotationSnapping = false;
            
            // Tam açıya snap
            playerBody.rotation = Quaternion.Euler(0, _targetRotation, 0);
        }
        
        private float GetCameraYAngle()
        {
            // Kameranın Y ekseni etrafındaki dönüş açısını hesapla
            var cameraForward = playerCamera.transform.forward;
            cameraForward.y = 0;
            return Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
        }
        #endregion

        #region Public Methods
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public float GetMoveSpeed()
        {
            return moveSpeed;
        }

        public Vector3 GetMovement()
        {
            return movement;
        }
        #endregion
        
    }
}