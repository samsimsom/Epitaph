using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerGravity : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;

        [Header("Gravity Settings")]
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float groundedGravity = -5f;
        [SerializeField] private float maxFallSpeed = -10f;

        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask groundLayers;

        [Header("Slope Settings")]
        [SerializeField] private float slopeForce = 5f;
        [SerializeField] private float slopeForceRayLength = 1.5f;
        [SerializeField] private float slideVelocity = 5f;
        [SerializeField] private float maxSlideSpeed = 10f;

        [Header("State (ReadOnly)")]
        [SerializeField] private float verticalVelocity;
        
        private float _verticalVelocity;
        private bool _isGrounded;
        
        private static float Gravity => Physics.gravity.y;

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
        }

        private void Update()
        {
            // Inspectordaki debug text updateti icin.
            verticalVelocity = _verticalVelocity;
            
            UpdateGroundedStatus();
            ApplyGravity();
        }

        private void UpdateGroundedStatus()
        {
            _isGrounded = PerformGroundCheck();
        }

        private bool PerformGroundCheck()
        {
            if (characterController == null)
                return false;
            
            var origin = characterController.transform.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var radius = characterController.radius;
            
            return Physics.CheckSphere(origin, radius, groundLayers);
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = groundedGravity;
            }
            else
            {
                _verticalVelocity += Gravity * gravityMultiplier * Time.deltaTime;
        
                // Terminal hız sınırı kontrolü
                if (_verticalVelocity < maxFallSpeed)
                    _verticalVelocity = maxFallSpeed;
            }

            var movement = Vector3.up * (_verticalVelocity * Time.deltaTime);
            HandleSlope(ref movement); // Eğimli yüzeyleri işle
            
            characterController.Move(movement);
        }

        private void HandleSlope(ref Vector3 moveDirection)
        {
            // Eğer karakter havadaysa, eğimi kontrol etme
            if (!_isGrounded)
                return;
            
            RaycastHit hit;
            
            // Aşağıya doğru bir ışın gönder ve eğimi kontrol et
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 
                       (characterController.height / 2f) + slopeForceRayLength, 
                       groundLayers))
            {
                var slopeNormal = hit.normal;
                var slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
        
                // Eğer eğim, CharacterController'ın tırmanabilme açısından fazlaysa kayma uygula
                if (slopeAngle > characterController.slopeLimit)
                {
                    // Eğim yönünü hesapla
                    var slopeDirection = Vector3.Cross(
                        Vector3.Cross(Vector3.up, slopeNormal), slopeNormal);
            
                    // Kayma miktarını hesapla
                    var slideAmount = Mathf.Clamp(
                        slideVelocity + (slopeAngle / 90f) * slopeForce, 
                        0, 
                        maxSlideSpeed
                    );
            
                    // Kayma yönünde hareket ekle
                    moveDirection += slopeDirection.normalized * (slideAmount * Time.deltaTime);
            
                    // Eğim üzerinde ekstra yerçekimi uygula
                    _verticalVelocity = groundedGravity * 2; // Eğim üzerinde daha güçlü yerçekimi
                }
            }
        }

        #region Public Methods
        public float GetVerticalVelocity() => _verticalVelocity;

        public void SetVerticalVelocity(float velocity) => _verticalVelocity = velocity;

        public bool IsGrounded() => _isGrounded;

        public void SetGroundedGravity(float gravity)
        {
            groundedGravity = gravity;
        }
        #endregion

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null) return;

            Gizmos.color = Color.red;
            
            var origin = characterController.transform.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var radius = characterController.radius;
            
            Gizmos.DrawWireSphere(origin, radius);
        }
        #endif
        
    }
}