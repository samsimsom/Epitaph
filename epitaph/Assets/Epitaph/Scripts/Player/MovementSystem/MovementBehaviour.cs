using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEditor;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // Alt behaviour'lar için manager
        private readonly PlayerBehaviourManager<MovementSubBehaviour> _movementBehaviourManager;
        
        // Sub behaviours
        public StateManager StateManager { get; private set; }
        // Gelecekte eklenebilecek diğer sub behaviours:
        // public GroundChecker GroundChecker { get; private set; }
        // public MovementPhysics MovementPhysics { get; private set; }
        
        // Movement Variables
        public float WalkSpeed = 2.5f;
        public float RunSpeed = 4.0f;
        public float CrouchSpeed = 1.5f;
        public float SpeedTransitionDuration = 0.1f;
        public float IdleTransitionDuration = 0.25f;

        // Jump Variables
        public float JumpForce = 4.0f;
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
        public bool IsGrounded { get; private set; }
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
            // Gelecekte diğer sub behaviours eklenebilir:
            // GroundChecker = _movementBehaviourManager.AddBehaviour(new GroundChecker(this, PlayerController));
            // MovementPhysics = _movementBehaviourManager.AddBehaviour(new MovementPhysics(this, PlayerController));
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
            CheckIsGrounded();
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
            // Movement debug bilgileri
            // GUI.Label(new Rect(10, 130, 300, 20), $"Is Grounded: {IsGrounded}");
            // GUI.Label(new Rect(10, 150, 300, 20), $"Vertical Movement: {VerticalMovement:F2}");
            // GUI.Label(new Rect(10, 170, 300, 20), $"Current Speed: {CurrentSpeed:F2}");
            // GUI.Label(new Rect(10, 190, 300, 20), $"Coyote Time: {CoyoteTimeCounter:F2}");
            
            _movementBehaviourManager?.ExecuteOnAll(b => b.OnGUI());
        }

        public override void OnDrawGizmos()
        {
            DrawCharacterControllerGizmo();
            DrawHasObstacleAboveForJumpGizmo();
            DrawCheckIsGroundedGizmo();
            DrawGroundNormalGizmo();
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
                position.y += 0.5f;
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
            else if (IsGrounded && VerticalMovement < 0)
            {
                VerticalMovement = -0.5f;
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

        private void CheckIsGrounded()
        {
            if (PlayerController.CharacterController == null)
            {
                IsGrounded = false;
                return;
            }

            var capsuleCollider = PlayerController.CharacterController;
            var center = capsuleCollider.bounds.center;
            var radius = capsuleCollider.radius * 0.9f;
            var rayDistance = capsuleCollider.bounds.extents.y + 0.1f;
            var layerMask = ~(1 << PlayerController.gameObject.layer);

            var origins = new[]
            {
                center,
                center + Vector3.forward * (radius * 0.5f),
                center + Vector3.back * (radius * 0.5f),
                center + Vector3.right * (radius * 0.5f),
                center + Vector3.left * (radius * 0.5f)
            };

            foreach (var origin in origins)
            {
                if (TryGroundNormalCheck(origin, rayDistance, layerMask, out var hitInfo))
                {
                    IsGrounded = true;
                    GroundNormal = hitInfo.normal;
                    return;
                }
            }

            IsGrounded = false;
            GroundNormal = Vector3.up;
        }

        private void CheckIsFalling()
        {
            IsFalling = !IsGrounded && VerticalMovement < 0;
        }

        private bool TryGroundNormalCheck(Vector3 origin, float rayDistance, LayerMask layerMask, out RaycastHit hitInfo)
        {
            return Physics.Raycast(origin, Vector3.down, out hitInfo, rayDistance, layerMask);
        }

        #endregion
        
        #if UNITY_EDITOR
        private void DrawGroundNormalGizmo()
        {
            if (!Application.isPlaying) return;
            
            var rayDistance = PlayerController.CharacterController.radius * 2f;
            var layerMask = ~LayerMask.GetMask("Player");
            var characterBaseWorld = PlayerController.CharacterController.transform.position + PlayerController.CharacterController.center - Vector3.up * 
                (PlayerController.CharacterController.height / 2f - PlayerController.CharacterController.radius);
            
            // Raycast origin pozisyonları
            var raycastOrigins = new[]
            {
                characterBaseWorld + (Vector3.left * PlayerController.CharacterController.radius),
                characterBaseWorld + (Vector3.right * PlayerController.CharacterController.radius),
                characterBaseWorld + (Vector3.forward * PlayerController.CharacterController.radius),
                characterBaseWorld + (Vector3.back * PlayerController.CharacterController.radius)
            };
            
            // Origin noktalarını çiz
            Gizmos.color = Color.cyan;
            foreach (var origin in raycastOrigins)
            {
                Gizmos.DrawWireSphere(origin, 0.025f);
            }
            
            // Her origin için raycast yap ve sonucu çiz
            foreach (var origin in raycastOrigins)
            {
                DrawRaycastResult(origin, rayDistance, layerMask);
            }
            
            // Hesaplanan ortalama ground normal'i çiz
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(characterBaseWorld, GroundNormal * 1f);
        }

        private void DrawRaycastResult(Vector3 origin, float rayDistance, LayerMask layerMask)
        {
            if (TryGroundNormalCheck(origin, rayDistance, layerMask, out var hitInfo))
            {
                // Hit varsa - yeşil çizgi ve mavi normal
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, hitInfo.point);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
            }
            else
            {
                // Hit yoksa - kırmızı çizgi
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + Vector3.down * rayDistance);
            }
        }
        
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

        private void DrawCheckIsGroundedGizmo()
        {
            var controller = PlayerController.CharacterController;
            var position = controller.transform.position;
            var center = controller.center;
            var height = controller.height;
            var radius = controller.radius;
            var origin = position + center - Vector3.up * (height / 2f);
            
            // Ana ground check
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(origin, radius);
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