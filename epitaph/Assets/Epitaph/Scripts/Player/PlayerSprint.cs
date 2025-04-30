using UnityEngine;
using UnityEngine.InputSystem;

namespace Epitaph.Scripts.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerSprint : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCrouch playerCrouch;

        [Header("Sprint Settings")]
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float sprintFOVChange = 10f;
        [SerializeField] private float fovChangeTime = 0.25f;
        [SerializeField] private float sprintStaminaUsage = 10f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRecoveryRate = 20f;
        [SerializeField] private float staminaRecoveryDelay = 1f;

        [Header("Debug")]
        [SerializeField] private bool isSprinting = false;
        [SerializeField] private float currentStamina;
        [SerializeField] private bool canSprint = true;

        private float _defaultMoveSpeed;
        private float _defaultFOV;
        private float _timeSinceLastSprint;
        private Camera _playerCamera;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            _defaultMoveSpeed = playerMovement.GetMoveSpeed();
            _playerCamera = playerMovement.GetPlayerCamera();
            if (_playerCamera != null)
            {
                _defaultFOV = _playerCamera.fieldOfView;
            }
            currentStamina = maxStamina;
        }

        private void Update()
        {
            UpdateStamina();
            UpdateFOV();
        }

        private void InitializeComponents()
        {
            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }

            if (playerCrouch == null)
            {
                playerCrouch = GetComponent<PlayerCrouch>();
            }
        }

        public void OnSprintPerformed(InputAction.CallbackContext context)
        {
            // Start sprint when button is pressed
            if (context.performed)
            {
                StartSprint();
            }
            // Stop sprint when button is released
            else if (context.canceled)
            {
                StopSprint();
            }
        }

        private void StartSprint()
        {
            // Check if player can sprint (not crouching and has stamina)
            if (playerCrouch != null && playerCrouch.IsCrouching())
            {
                return;
            }

            if (!canSprint || currentStamina <= 0)
            {
                return;
            }

            isSprinting = true;
            playerMovement.SetMoveSpeed(sprintSpeed);
        }

        private void StopSprint()
        {
            if (!isSprinting) return;
            
            isSprinting = false;
            playerMovement.SetMoveSpeed(_defaultMoveSpeed);
        }

        private void UpdateStamina()
        {
            // If sprinting, reduce stamina
            if (isSprinting)
            {
                currentStamina -= sprintStaminaUsage * Time.deltaTime;
                _timeSinceLastSprint = 0f;
                
                // If stamina is depleted, stop sprinting
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    canSprint = false;
                    StopSprint();
                }
            }
            // If not sprinting, recover stamina after delay
            else
            {
                _timeSinceLastSprint += Time.deltaTime;
                
                if (_timeSinceLastSprint >= staminaRecoveryDelay)
                {
                    currentStamina += staminaRecoveryRate * Time.deltaTime;
                    
                    // If stamina is recovered enough, allow sprinting again
                    if (currentStamina > maxStamina * 0.15f)
                    {
                        canSprint = true;
                    }
                    
                    // Cap stamina at max
                    if (currentStamina > maxStamina)
                    {
                        currentStamina = maxStamina;
                    }
                }
            }
        }

        private void UpdateFOV()
        {
            if (_playerCamera == null) return;

            float targetFOV = isSprinting ? _defaultFOV + sprintFOVChange : _defaultFOV;
            _playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, targetFOV, fovChangeTime * Time.deltaTime * 5f);
        }

        public bool IsSprinting()
        {
            return isSprinting;
        }

        public float GetStaminaPercentage()
        {
            return currentStamina / maxStamina;
        }

        // If the player starts crouching while sprinting, stop the sprint
        private void OnEnable()
        {
            if (playerCrouch != null)
            {
                playerCrouch.OnCrouchStateChanged += HandleCrouchStateChanged;
            }
        }

        private void OnDisable()
        {
            if (playerCrouch != null)
            {
                playerCrouch.OnCrouchStateChanged -= HandleCrouchStateChanged;
            }
        }

        private void HandleCrouchStateChanged(bool isCrouching)
        {
            if (isCrouching && isSprinting)
            {
                StopSprint();
            }
        }
    }
}