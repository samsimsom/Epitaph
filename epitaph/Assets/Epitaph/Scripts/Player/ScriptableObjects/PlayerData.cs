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
        public float fatiqueIncreaseRate;
        public float fatiqueExhaustionThreshold;
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
        public float frequenct = 10.0f;
        public float smooth = 10.0f;
        public float treshold = 2.0f;

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
    }
}