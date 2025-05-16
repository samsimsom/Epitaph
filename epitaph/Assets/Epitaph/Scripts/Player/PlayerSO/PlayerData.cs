using UnityEngine;

namespace Epitaph.Scripts.Player.PlayerSO
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Player/PlayerData", order = 0)]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement Data")]
        public float moveSpeed;
        
        [Header("Look Data")]
        public Vector2 lookSensitivity;
        public float referanceAspect;

        [Header("Gravity Data")] 
        public float gravityMultiplier;
        public float groundedGravity;
        public float maxFallSpeed;
        public LayerMask groundLayers;
        
        public float slopeForce;
        public float slopeForceRayLength;
        public float slideVelocity;
        public float maxSlideSpeed;
        
        [Header("Crouch Data")]
        [Header("Crouch Settings")]
        public float crouchHeight = 1.0f;
        public float standingHeight = 2.0f;
        public float crouchSpeed = 2.0f;
        public float crouchCameraYOffset = -0.5f;
        public float standingCameraYOffset;
        public float crouchTransitionTime = 0.2f;

        [Header("Ceil Check Settings")]
        public LayerMask ceilingLayers;
        public float ceilingCheckDistance = 0.5f;
    }
}