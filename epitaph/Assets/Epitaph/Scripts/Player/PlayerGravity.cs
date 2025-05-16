using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerGravity : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController;

        [Header("State (ReadOnly)")]
        [SerializeField] private float verticalVelocity;
        
        private float _groundedGravity;
        private float _verticalVelocity;
        private bool _isGrounded;
        
        private static float Gravity => Physics.gravity.y;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            PlayerCrouch.OnChangeGroundedGravity += groundedGravity =>
            {
                _groundedGravity = groundedGravity;
            };
        }

        private void OnDisable()
        {
            PlayerCrouch.OnChangeGroundedGravity -= groundedGravity =>
            {
                groundedGravity = _groundedGravity;
            };
        }

        private void Initialize()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            _groundedGravity = playerData.groundedGravity;
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
            playerData.isGrounded = _isGrounded;
        }

        private bool PerformGroundCheck()
        {
            if (characterController == null)
                return false;
            
            var origin = characterController.transform.position 
                         + characterController.center 
                         + Vector3.down * (characterController.height / 2f);
            var radius = characterController.radius;
            
            return Physics.CheckSphere(origin, radius, playerData.groundLayers);
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = _groundedGravity;
            }
            else
            {
                _verticalVelocity += Gravity * playerData.gravityMultiplier * Time.deltaTime;
        
                // Terminal hız sınırı kontrolü
                if (_verticalVelocity < playerData.maxFallSpeed)
                    _verticalVelocity = playerData.maxFallSpeed;
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
                       (characterController.height / 2f) + playerData.slopeForceRayLength, 
                       playerData.groundLayers))
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
                        playerData.slideVelocity + (slopeAngle / 90f) * playerData.slopeForce, 
                        0, playerData.maxSlideSpeed
                    );
            
                    // Kayma yönünde hareket ekle
                    moveDirection += slopeDirection.normalized * (slideAmount * Time.deltaTime);
            
                    // Eğim üzerinde ekstra yerçekimi uygula
                    _verticalVelocity = playerData.groundedGravity * 2;
                }
            }
        }

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