using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // State Variables
        private BaseState _currentState;
        private StateFactory _states;
        
        // Movement Variables
        public float WalkSpeed = 2.5f;
        public float RunSpeed = 3.4f;
        public float CrouchSpeed = 1.5f;

        // Jump Variables
        public float JumpForce = 6.0f;
        public float Gravity = 20.0f;
        private float _verticalVelocity;
        public float coyoteTime = 0.2f; // Saniye cinsinden coyote süresi
        public float coyoteTimeCounter = 0f;


        // Crouch Variables
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);
        public float NormalCameraHeight = 1.5f;
        public float CrouchCameraHeight = 0.7f;
        private bool _isCrouching;

        // Input Variables
        private Vector2 _currentMovementInput;

        // Getters & Setters
        public BaseState CurrentState 
        { 
            get => _currentState;
            set => _currentState = value;
        }
        public bool IsCrouching
        {
            get => _isCrouching;
            set => _isCrouching = value;
        }
        public float CurrentMovementY
        {
            get => _verticalVelocity;
            set => _verticalVelocity = value;
        }
        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }

        // Crouch için coroutine süresi ve coroutine referansları burada tanımlı olacak
        // ama mantık CrouchState'e taşındı.
        public float CrouchTransitionDuration = 0.2f;

        // ---------------------------------------------------------------------------- //
        
        public MovementBehaviour(PlayerController playerController)
            : base(playerController) { }
        
        // ---------------------------------------------------------------------------- //
        
        public override void Awake()
        {
            _states = new StateFactory(this);
            _currentState = _states.Idle();
            _currentState.EnterState();
        }
        
        public override void Update()
        {
            _currentState.UpdateState();
            HandleMovement();
            HandleGravity();
            
            // Coyote time yönetimi
            if (PlayerController.CharacterController.isGrounded)
            {
                coyoteTimeCounter = coyoteTime; // Yere değince yenile
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
        }
        
        public override void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        // ---------------------------------------------------------------------------- //

        private void HandleMovement()
        {
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            moveDirection = PlayerController.PlayerCamera.transform.TransformDirection(moveDirection);
            moveDirection.y = _verticalVelocity;
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);
        }
        
        private void HandleGravity()
        {
            if (PlayerController.CharacterController.isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -1f;
            }
            else
            {
                _verticalVelocity -= Gravity * Time.deltaTime;
            }
        }
        
        // ---------------------------------------------------------------------------- //
        
        
        
        // ---------------------------------------------------------------------------- //
        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (PlayerController.CharacterController == null) return;

            // Renk ve şeffaflık
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 1.0f);

            // Capsule bilgileri
            var center = PlayerController.CharacterController.transform.position 
                         + PlayerController.CharacterController.center;
            var height = PlayerController.CharacterController.height;
            var radius = PlayerController.CharacterController.radius;

            // Kapsülün üst ve alt merkezleri
            var cylinderHeight = Mathf.Max(0, height / 2f - radius);
            var up = PlayerController.CharacterController.transform.up;

            var top = center + up * cylinderHeight;
            var bottom = center - up * cylinderHeight;

            // Kapsül çizimi
            Gizmos.DrawWireSphere(top, radius);       // Üst küre
            Gizmos.DrawWireSphere(bottom, radius);    // Alt küre
            Gizmos.DrawLine(top + PlayerController.CharacterController.transform.right * radius, bottom 
                + PlayerController.CharacterController.transform.right * radius);
            Gizmos.DrawLine(top - PlayerController.CharacterController.transform.right * radius, bottom 
                - PlayerController.CharacterController.transform.right * radius);
            Gizmos.DrawLine(top + PlayerController.CharacterController.transform.forward * radius, bottom 
                + PlayerController.CharacterController.transform.forward * radius);
            Gizmos.DrawLine(top - PlayerController.CharacterController.transform.forward * radius, bottom
                - PlayerController.CharacterController.transform.forward * radius);
        }
#endif

    }
}