using UnityEngine;

namespace Epitaph.Scripts.Player.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Systems/Movement System", order = 0)]
    public class PlayerData : ScriptableObject
    {
        [Header("Condition")]
        public float health;
        public float maxHealth;
        public float healthIncreaseRate;
        public float healthDecreaseRate;
        [Space]
        public float stamina;
        public float maxStamina;
        public float staminaIncreaseRate;
        public float staminaDecreaseRate;
        public float staminaRecoveryDelay;
        public float staminaEnoughPercentage;
        [Space]
        public float hunger;
        public float maxHunger;
        public float hungerIncreaseRate;
        public float hungerStarvingThreshold;
        public float hungerDefaultModifier;
        public float hungerRunningModifier;
        [Space]
        public float thirst;
        public float maxThirst;
        public float thirstIncreaseRate;
        public float thirstDehydrationThreshold;
        public float thirstDefaultModifier;
        public float thirstRunningModifier;
        [Space]
        public float fatigue;
        public float maxFatigue;
        public float fatigueIncreaseRate;
        public float fatigueExhaustionThreshold;
        public float fatigueRecoveryPerHour;

        [Header("Movement Data")] 
        public float currentSpeed;
        public float walkSpeed;
        public Vector3 currentVelocity;
        
        [Header("Look Data")]
        public Vector2 lookSensitivity;
        public float referanceAspect;
        
        [Header("Headbob Data")]
        public float amount = 0.02f;
        public float frequency = 10.0f;
        public float smooth = 10.0f;
        public float threshold = 2.0f;

        [Header("Gravity Data")] 
        public bool isGrounded;
        public float gravityMultiplier;
        public float groundedGravity;
        public float maxFallSpeed;
        public float verticalVelocity;
        public LayerMask groundLayers;
        [Space] 
        public bool isFalling;
        public float fallThreshold;
        [Space]
        public float slopeClimbThreshold = 0.5f;
        public float slopeForce;
        public float slopeForceRayLength;
        public float slideVelocity;
        public float maxSlideSpeed;
        
        [Header("Jump Data")]
        public float jumpHeight = 2f;
        public float jumpCooldown = 0.1f;
        public float jumpBufferTime = 0.2f;
        
        [Space]
        public float coyoteTime = 0.2f;
        public bool useCoyoteTime = true;
        
        [Header("Crouch Data")]
        public bool isCrouching;
        public float crouchHeight = 1.0f;
        public float standingHeight = 2.0f;
        public float crouchSpeed = 2.0f;
        public float crouchCameraYOffset = -0.5f;
        public float standingCameraYOffset;
        public float crouchTransitionTime = 0.2f;
        public float crouchGroundedGravity = -100f;
        
        [Space]
        public LayerMask ceilingLayers;
        public float ceilingCheckDistance = 0.3f;
        
        [Header("Sprint Settings")]
        public bool isSprinting;
        public float sprintSpeed = 10f;
        
        [Header("Interaction Data")]
        public float interactionDistance = 3f;
        public LayerMask interactableLayer;
        public float raycastInterval = 0.05f;
        
        [Space]
        public bool showDebugGizmos = true;
        public Color hitGizmoColor = Color.yellow;
        public Color gizmoColor = Color.red;
        
        // ---------------------------------------------------------------------------- //
        
        public void InitializeDefaults()
        {
            // Condition
            health = 100f;
            maxHealth = 100f;
            healthIncreaseRate = 1f;
            healthDecreaseRate = 5f;

            stamina = 100f;
            maxStamina = 100f;
            staminaIncreaseRate = 10f;
            staminaDecreaseRate = 20f;
            staminaRecoveryDelay = 2f;
            staminaEnoughPercentage = 0.3f;

            hunger = 0f;
            maxHunger = 100f;
            hungerIncreaseRate = 1f;
            hungerStarvingThreshold = 20f;
            hungerDefaultModifier = 1f;
            hungerRunningModifier = 1.5f;

            thirst = 0f;
            maxThirst = 100f;
            thirstIncreaseRate = 1f;
            thirstDehydrationThreshold = 20f;
            thirstDefaultModifier = 1f;
            thirstRunningModifier = 1.5f;

            fatigue = 0f;
            maxFatigue = 100f;
            fatigueIncreaseRate = 1f;
            fatigueExhaustionThreshold = 80f;
            fatigueRecoveryPerHour = 10f;

            // Movement Data
            walkSpeed = 5f;

            // Look Data
            lookSensitivity = new Vector2(20f, 20f);
            referanceAspect = 16f / 9f;

            // Headbob Data
            amount = 0.02f;
            frequency = 10.0f;
            smooth = 10.0f;
            threshold = 2.0f;

            // Gravity Data
            isGrounded = false;
            gravityMultiplier = 1f;
            groundedGravity = -9.81f;
            maxFallSpeed = -50f;
            verticalVelocity = 0f;

            isFalling = false;
            fallThreshold = -5f;

            slopeClimbThreshold = 0.5f;
            slopeForce = 10f;
            slopeForceRayLength = 1.5f;
            slideVelocity = 5f;
            maxSlideSpeed = 15f;

            // Jump Data
            jumpHeight = 2f;
            jumpCooldown = 0.1f;
            jumpBufferTime = 0.2f;

            coyoteTime = 0.2f;
            useCoyoteTime = true;

            // Crouch Data
            isCrouching = false;
            crouchHeight = 1.0f;
            standingHeight = 2.0f;
            crouchSpeed = 2.0f;
            crouchCameraYOffset = -0.5f;
            standingCameraYOffset = 0f;
            crouchTransitionTime = 0.2f;
            crouchGroundedGravity = -100f;
            // ceilingLayers Inspector'dan atanır
            ceilingCheckDistance = 0.3f;

            // Sprint Settings
            isSprinting = false;
            sprintSpeed = 10f;

            // Interaction Data
            interactionDistance = 3f;
            raycastInterval = 0.05f;

            showDebugGizmos = true;
            hitGizmoColor = Color.yellow;
            gizmoColor = Color.red;
        }
    }
}