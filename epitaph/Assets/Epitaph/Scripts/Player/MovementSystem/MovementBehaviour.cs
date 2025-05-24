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
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 1.5f;

        // Jump Variables
        public float JumpForce = 6.0f;
        public float AirControlFactor = 1.25f;
        public float Gravity = 20.0f;
        public float CoyoteTime = 0.2f; // Saniye cinsinden coyote süresi
        public float CoyoteTimeCounter;

        private float _verticalVelocity;

        // Crouch Variables
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);
        public float NormalCameraHeight = 1.5f;
        public float CrouchCameraHeight = 0.7f;
        public float CrouchTransitionDuration = 0.2f;
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
                CoyoteTimeCounter = CoyoteTime; // Yere değince yenile
            }
            else
            {
                CoyoteTimeCounter -= Time.deltaTime;
            }
        }
        
        public override void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        // ---------------------------------------------------------------------------- //

        private void HandleMovement()
        {
            // XZ düzleminde hareket (normalize ile hızlı yön değişimlerinde hız kaybı engellenir)
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            // if (moveDirection.magnitude > 1)
            //     moveDirection.Normalize();

            // Kamera yönüne göre döndür
            moveDirection = PlayerController.PlayerCamera.transform.TransformDirection(moveDirection);

            // Dikey hızı hareket vektörüne uygula
            moveDirection.y = _verticalVelocity;

            // Hareket uygula
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);

            // Yere değerse dikey hızı hafifçe sabitle, böylece character controller'ın "yerde kayma bugı" azalır
            if (PlayerController.CharacterController.isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -1f;
            }
        }

        private void HandleGravity()
        {
            if (PlayerController.CharacterController.isGrounded)
            {
                // Eğer yere yeni değildiysek ve aşağı yönde hareket ediyorsak hızı sıfırla
                if (_verticalVelocity < 0)
                    _verticalVelocity = -1f;
            }
            else
            {
                // Daha kontrollü bir düşüş eğrisi için gravity factor'u siz ayarlayabilirsiniz.
                var gravityMultiplier = 1.0f;
                // Eğer jump tusu bırakıldıysa veya oyuncu alçalmaya başladıysa gravity hızlanabilir
                if (_verticalVelocity < 0)
                    gravityMultiplier = 1.5f; // Daha gerçekçi düşüş için
                _verticalVelocity -= Gravity * gravityMultiplier * Time.deltaTime;
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