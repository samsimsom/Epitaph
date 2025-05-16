#if true
using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Epitaph.Scripts.Player
{
    public class PlayerSprint : MonoBehaviour
    {
        public static event Action<float> OnChangeSprintSpeed;
        
        [Header("Data")]
        [SerializeField] private PlayerData playerData;

        [Header("Debug")]
        [SerializeField] private float currentStamina;
        [SerializeField] private bool canSprint = true;

        private float _defaultMoveSpeed;
        private float _timeSinceLastSprint;
        private bool _isSprintKeyHeld;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            PlayerInput.OnSprintActivated += OnSprintActivated;
            PlayerInput.OnSprintDeactivated += OnSprintDeactivated;
            PlayerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
        }

        private void OnDisable()
        {
            PlayerInput.OnSprintActivated -= OnSprintActivated;
            PlayerInput.OnSprintDeactivated -= OnSprintDeactivated;
            PlayerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
        }
        
        private void Start()
        {
            // _defaultMoveSpeed = playerMove.GetMoveSpeed();
            currentStamina = playerData.maxStamina;
        }

        private void Update()
        {
            if (_isSprintKeyHeld && !playerData.isSprinting)
            {
                // Yerdeyiz, sprint tuşu basılı ve sprint yapmıyoruz
                // Sprint'i tekrar başlatmayı dene
                TryStartSprint();
            }
            
            // Stamina güncellemelerini yap
            UpdateStamina();
        }

        private void Initialize() { }

        private void OnSprintActivated()
        {
            // Start sprint when button is pressed
            _isSprintKeyHeld = true;
            TryStartSprint();
        }

        private void OnSprintDeactivated()
        {
            // Stop sprint when button is released
            _isSprintKeyHeld = false;
            StopSprint();
        }
        
        private void TryStartSprint()
        {
            // Eğer oyuncu yerdeyse ve sprint yapabiliyorsa
            if (!playerData.isGrounded || !canSprint || !(currentStamina > 0)) return;
            
            // Çömelmiyorsa
            if (playerData.isCrouching) return;
            
            playerData.isSprinting = true;
            OnChangeSprintSpeed?.Invoke(playerData.sprintSpeed);
        }

        private void StopSprint()
        {
            if (!playerData.isSprinting) return;
            
            playerData.isSprinting = false;
            OnChangeSprintSpeed?.Invoke(playerData.walkSpeed);
        }

        private void UpdateStamina()
        {
            // If sprinting, reduce stamina
            if (playerData.isSprinting)
            {
                currentStamina -= playerData.sprintStaminaUsage * Time.deltaTime;
                _timeSinceLastSprint = 0f;
                
                // If stamina is depleted, stop sprinting
                if (!(currentStamina <= 0)) return;
                
                currentStamina = 0;
                canSprint = false;
                StopSprint();
            }
            // If not sprinting, recover stamina after delay
            else
            {
                _timeSinceLastSprint += Time.deltaTime;

                if (!(_timeSinceLastSprint >= playerData.staminaRecoveryDelay)) return;
                
                currentStamina += playerData.staminaRecoveryRate * Time.deltaTime;
                    
                // If stamina is recovered enough, allow sprinting again
                if (currentStamina > playerData.maxStamina * playerData.staminaEnoughPercentage)
                {
                    canSprint = true;
                }
                    
                // Cap stamina at max
                if (currentStamina > playerData.maxStamina)
                {
                    currentStamina = playerData.maxStamina;
                }
            }
            
        }
        
        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (!isCrouching || !playerData.isSprinting) return;
            
            StopSprint();
            OnChangeSprintSpeed?.Invoke(playerData.crouchSpeed);
        }
        
    }
}
#endif