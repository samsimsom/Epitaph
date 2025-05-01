using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
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
        
        private Vector3 _movement;
        private float _lastCameraAngle;
        private float _targetRotation;
        private bool _isRotationSnapping;
        
        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            var playerHeight = characterController.height / 2f + characterController.skinWidth;
            transform.position = new Vector3(0, playerHeight, 0);
            _lastCameraAngle = GetCameraYAngle();
        }

        private void Update()
        {
            HandleRotation();
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
            _movement = direction * (moveSpeed * Time.deltaTime);
            characterController.Move(_movement);
        }

        private void RotateTowardsCameraDirection()
        {
            // Kameranın baktığı yöne göre dönüş açısını hesapla
            float cameraAngle = GetCameraYAngle();
            _targetRotation = cameraAngle;
            
            // Hızlı dönüş
            float currentAngle = Mathf.LerpAngle(playerBody.eulerAngles.y, cameraAngle, 
                movingRotationSpeed * Time.deltaTime);
            playerBody.rotation = Quaternion.Euler(0, currentAngle, 0);
        }
        
        private void CheckForCameraAngleSnap()
        {
            var currentCameraAngle = GetCameraYAngle();
            
            // Kamera açısı değişimini kontrol et
            var angleDifference = Mathf.Abs(Mathf.DeltaAngle(_lastCameraAngle, currentCameraAngle));
            
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
            if (_isRotationSnapping)
            {
                // Yavaşça hedef açıya dön
                float currentAngle = Mathf.LerpAngle(playerBody.eulerAngles.y, _targetRotation, idleRotationSpeed * Time.deltaTime);
                playerBody.rotation = Quaternion.Euler(0, currentAngle, 0);
                
                // Dönüş tamamlandı mı kontrol et
                if (Mathf.Abs(Mathf.DeltaAngle(playerBody.eulerAngles.y, _targetRotation)) < 0.5f)
                {
                    _isRotationSnapping = false;
                    playerBody.rotation = Quaternion.Euler(0, _targetRotation, 0); // Tam açıya snap
                }
            }
        }
        
        private float GetCameraYAngle()
        {
            // Kameranın Y ekseni etrafındaki dönüş açısını hesapla
            Vector3 cameraForward = playerCamera.transform.forward;
            cameraForward.y = 0;
            return Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
        }
        
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
            return _movement;
        }

        public Camera GetPlayerCamera()
        {
            return playerCamera;
        }
        
        public CharacterController GetCharacterController()
        {
            return characterController;
        }
    }
}