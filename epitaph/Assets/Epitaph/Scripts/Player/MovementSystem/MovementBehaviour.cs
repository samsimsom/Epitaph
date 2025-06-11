using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEditor;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // Alt behaviour'lar için manager
        private readonly PlayerBehaviourManager<MovementSubBehaviour> _movementBehaviourManager;

        #region Sub behaviours

        public StateManager StateManager { get; private set; }
        public PlayerStepDetection PlayerStepDetection { get; private set; }
        public PlayerGroundDetection PlayerGroundDetection { get; private set; }

        #endregion
        
        // Movement Variables
        public float WalkSpeed = 2.5f;
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 1.5f;
        public float SpeedTransitionDuration = 0.1f;
        public float IdleTransitionDuration = 0.25f;

        // Jump Variables
        public float JumpForce = 5.0f;
        public float AirControlFactor = 1.25f;
        public float Gravity = 20.0f;
        
        // Coyote Time Counter
        public float CoyoteTime = 0.2f;
        public float CoyoteTimeCounter;

        // Gravity Variables
        public float VerticalMovementLimit = -10.0f;

        // Crouch Variables
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public float NormalCameraHeight = 1.5f;
        public float CrouchCameraHeight = 0.7f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);

        // Getters & Setters (StateManager'dan erişim için)
        public bool IsWalking { get; internal set; }
        public bool IsRunning { get; internal set; }
        public bool IsFalling { get; internal set; }
        public bool IsJumping { get; internal set; }
        public bool IsCrouching { get; internal set; }
        public bool IsGrounded => PlayerGroundDetection.IsGrounded;
        public Vector3 GroundNormal { get; private set; }
        
        public Vector3 CapsulVelocity { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float VerticalMovement { get; internal set; }

        public float AppliedMovementX { get; internal set; }
        public float AppliedMovementZ { get; internal set; }

        // ---------------------------------------------------------------------------- //
        
        public MovementBehaviour(PlayerController playerController)
            : base(playerController)
        {
            _movementBehaviourManager = new PlayerBehaviourManager<MovementSubBehaviour>(playerController);
            InitializeBehaviours();
        }
        
        // ---------------------------------------------------------------------------- //
        
        private void InitializeBehaviours()
        {
            StateManager = _movementBehaviourManager.AddBehaviour(new StateManager(this, PlayerController));
            PlayerStepDetection = _movementBehaviourManager.AddBehaviour(new PlayerStepDetection(this, PlayerController));
            PlayerGroundDetection = _movementBehaviourManager.AddBehaviour(new PlayerGroundDetection(this, PlayerController));
        }
        
        // ---------------------------------------------------------------------------- //
        
        #region Unity Lifecycle Methods
        
        public override void Awake()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.Awake());
        }

        public override void OnEnable()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnEnable());
        }

        public override void Start()
        {
            AdjustPlayerPosition();
            _movementBehaviourManager?.ExecuteOnAll(b => b.Start());
        }

        public override void Update()
        {
            CheckIsFalling();
            HandleMovement();
            ManageCoyoteTime();
            
            _movementBehaviourManager?.ExecuteOnAll(b => b.Update());
        }

        public override void LateUpdate()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.LateUpdate());
        }

        public override void FixedUpdate()
        {
            HandleGravity();
            _movementBehaviourManager?.ExecuteOnAll(b => b.FixedUpdate());
        }

        public override void OnDisable()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnDisable());
        }

        public override void OnDestroy()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnDestroy());
        }

#if UNITY_EDITOR
        public override void OnGUI()
        {
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnGUI());
        }

        public override void OnDrawGizmos()
        {
            DrawCharacterControllerGizmo();
            DrawHasObstacleAboveForJumpGizmo();
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnDrawGizmos());
        }
#endif

        #endregion

        // ---------------------------------------------------------------------------- //
        
        #region Movement Methods

        private void AdjustPlayerPosition()
        {
            if (PlayerController.CharacterController != null)
            {
                var position = PlayerController.transform.position;
                position.y += PlayerController.CharacterController.skinWidth;
                PlayerController.transform.position = position;
            }
        }

        private void ManageCoyoteTime()
        {
            if (IsGrounded)
            {
                CoyoteTimeCounter = CoyoteTime;
            }
            else
            {
                CoyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            if (PlayerController.CharacterController != null)
            {
                CapsulVelocity = PlayerController.CharacterController.velocity;
                CurrentSpeed = new Vector3(CapsulVelocity.x, 0, CapsulVelocity.z).magnitude;
                
                // Step detection için input al (InputBehaviour'dan gelecek)
                var moveInput = new Vector2(AppliedMovementX, AppliedMovementZ);
                PlayerStepDetection.HandleStepOffset(moveInput);

                ApplyMovement();
            }
        }
        
        private void ApplyMovement()
        {
            if (PlayerController.CharacterController == null) return;
    
            Vector3 movement;

            // Kamera yönüne göre hareket vektörünü hesapla
            var cameraForward = PlayerController.PlayerCamera.transform.forward;
            var cameraRight = PlayerController.PlayerCamera.transform.right;
        
            // Y bileşenini sıfırla (sadece yatay hareket)
            cameraForward.y = 0f;
            cameraRight.y = 0f;
        
            // Normalize et
            cameraForward.Normalize();
            cameraRight.Normalize();
        
            // Kamera yönüne göre hareket hesapla
            movement = cameraRight * AppliedMovementX + cameraForward * AppliedMovementZ;

    
            // Dikey hareketi ekle
            movement.y = VerticalMovement;
    
            // Hareketi uygula
            PlayerController.CharacterController.Move(movement * Time.deltaTime);
        }

        private void HandleGravity()
        {
            if (!IsGrounded && VerticalMovement > VerticalMovementLimit)
            {
                VerticalMovement -= Gravity * Time.fixedDeltaTime;
            }
        }

        public bool HasObstacleAboveForJump()
        {
            if (PlayerController.CharacterController == null) return true;

            var capsuleCollider = PlayerController.CharacterController;
            var center = capsuleCollider.bounds.center;
            var radius = capsuleCollider.radius * 0.9f;
            var height = capsuleCollider.height;
            var checkDistance = height * 0.5f + JumpForce * 0.1f;

            var layerMask = ~(1 << PlayerController.gameObject.layer);

            return Physics.CheckCapsule(
                center,
                center + Vector3.up * checkDistance,
                radius,
                layerMask
            );
        }

        private void CheckIsFalling()
        {
            IsFalling = !PlayerGroundDetection.IsGrounded && VerticalMovement < 0;
        }

        #endregion
        
    #if UNITY_EDITOR
        
        private void DrawCharacterControllerGizmo()
        {
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

        private void DrawHasObstacleAboveForJumpGizmo()
        {
            // Controller parametreleri
            var controller = PlayerController.CharacterController;
            var radius = controller.radius;
            var position = controller.transform.position + controller.center;
            var height = controller.height;
            var checkDistance = height * 1.25f - height; // %25 daha yüksek mesafe

            // Cast başlangıç ve bitiş pozisyonları (HasObstacleAboveForJump() ile aynı)
            var castStart = position + Vector3.up * (height / 2f - radius);
            // var castEnd = castStart + Vector3.up * checkDistance;

            // Mevcut kapsülü çizer (mavi)
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.6f);
            DrawWireCapsule(
                position + Vector3.up * (-(height / 2f - radius)),
                position + Vector3.up * (height / 2f - radius),
                radius
            );

            // Yukarıya yapılacak cast kapsülünü çizer (sarı)
            Gizmos.color = Color.yellow;
            DrawWireCapsule(
                castStart,
                castStart + Vector3.up * checkDistance,
                radius
            );

            // İstenirse çarpışma varsa kırmızı bir nokta gösterebiliriz:
            var layerMask = ~LayerMask.GetMask("Player");
            if (Physics.CapsuleCast(
                    castStart,
                    castStart,
                    radius,
                    Vector3.up,
                    out var hit,
                    checkDistance,
                    layerMask
                ))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.06f);
            }
        }
        
        private void DrawWireCapsule(Vector3 start, Vector3 end, float radius)
        {
            // Unity 2020 ve sonrası için kullanışlı bir API yoksa:
            Handles.DrawWireDisc(start, (end - start).normalized, radius);
            Handles.DrawWireDisc(end, (end - start).normalized, radius);
            Handles.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
            Handles.DrawLine(start - Vector3.right * radius, end - Vector3.right * radius);
            Handles.DrawLine(start + Vector3.forward * radius, end + Vector3.forward * radius);
            Handles.DrawLine(start - Vector3.forward * radius, end - Vector3.forward * radius);
        }
        
#endif
        
    }
}