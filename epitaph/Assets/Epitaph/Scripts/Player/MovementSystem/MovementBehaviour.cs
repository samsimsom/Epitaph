using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEditor;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // State Variables
        private StateFactory _states;
        
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

        // Getters & Setters
        public StateBase CurrentState { get; internal set; }
        
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
        
        // Movement efficiency based on life stats
        public float MovementEfficiency { get; set; } = 1f;
        
        // Character Controller Variables
        private CharacterController _cController;
        private Vector3 _cPosition;
        private Vector3 _cCenter;
        private float _cHeight;
        private float _cRaius;

        // ---------------------------------------------------------------------------- //
        
        public MovementBehaviour(PlayerController playerController)
            : base(playerController) { }
        
        // ---------------------------------------------------------------------------- //
        
        public override void Awake()
        {
            _states = new StateFactory(this);
            CurrentState = _states.Idle();
            CurrentState.EnterState();
        }

        public override void Start()
        {
            AdjustPlayerPosition();
        }

        // Update metodunda çağırın
        public override void Update()
        {
            // Character Controller Variables
            // ------------------------------------------------------------------------ //
            _cController = PlayerController.CharacterController;
            _cPosition = _cController.transform.position;
            _cCenter = _cController.center;
            _cHeight = _cController.height;
            _cRaius = _cController.radius;
            // ------------------------------------------------------------------------ //
            
            CheckIsGrounded();
            CheckIsFalling();
            CurrentState.UpdateState();
            HandleMovement();
            ManageCoyoteTime();
        }

        public override void FixedUpdate()
        {
            HandleGravity();
            CurrentState.FixedUpdateState();
        }

        // ---------------------------------------------------------------------------- //

        private void AdjustPlayerPosition()
        {
            // CharacterControllerdan gelen skinWidth offseti pozisyona ekleniyor.
            var playerHeight = PlayerController.CharacterController.skinWidth;
            PlayerController.transform.position += new Vector3(0, playerHeight, 0);
        }
        
        private void ManageCoyoteTime()
        {
            // Coyote time yönetimi
            if (IsGrounded)
            {
                CoyoteTimeCounter = CoyoteTime; // Yere değince yenile
            }
            else
            {
                CoyoteTimeCounter -= Time.deltaTime;
            }
        }
        
        // ---------------------------------------------------------------------------- //
        
        // Modified HandleMovement method
        private void HandleMovement()
        {
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            
            // Apply movement efficiency
            moveDirection *= MovementEfficiency;
            
            // Kamera yönüne göre döndür
            var cam = PlayerController.PlayerCamera.transform;
            var forward = Vector3.ProjectOnPlane(cam.forward, 
                Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(cam.right, 
                Vector3.up).normalized;
            moveDirection = moveDirection.z * forward + moveDirection.x * right;
            
            // Dikey hızı hareket vektörüne uygula
            moveDirection.y = VerticalMovement;

            // Hareket uygula
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);
            
            // Update CurrentVelocity
            CapsulVelocity = PlayerController.CharacterController.velocity;
            CurrentSpeed = CapsulVelocity.magnitude;
        }

        // ---------------------------------------------------------------------------- //

        private void HandleGravity()
        {
            // Eğer yerdeyse ve vertical movement negatifse sıfırla
            if (IsGrounded && VerticalMovement < 0f)
            {
                VerticalMovement = -5.0f;
                return; // Aşağıdaki yerçekimi uygulaması gereksiz
            }

            var gravityMultiplier = VerticalMovement < 0 ? 1.2f : 1.0f;
            VerticalMovement -= Gravity * 0.85f * gravityMultiplier * Time.fixedDeltaTime;
            VerticalMovement = Mathf.Max(VerticalMovement, VerticalMovementLimit);
        }
        
        // ---------------------------------------------------------------------------- //
        
        // Karakterin üstünde zıplamayı engelleyen bir yüksek engel var mı?
        public bool HasObstacleAboveForJump()
        {
            var controller = PlayerController.CharacterController;
            var radius = controller.radius;
            var position = controller.transform.position + controller.center;
            var height = controller.height;
            var checkDistance = height * 1.25f - height; // %25 daha yüksek mesafe
            var castStart = position + Vector3.up * (height / 2f - radius);

            // Yalnızca Player layer'ı hariç 
            var layerMask = ~LayerMask.GetMask("Player");

            if (Physics.CapsuleCast(castStart, castStart, radius, 
                    Vector3.up, out var hit, checkDistance, layerMask))
            {
                Debug.Log($"<color=orange>Cannot Jump</color>, hit: {hit.collider.name}");
                return false;
            }

            return true;
        }
        
        // ---------------------------------------------------------------------------- //
        
        // Custom Grounded Check
        private void CheckIsGrounded()
        {
            var controller = PlayerController.CharacterController;
            var controllerPosition = controller.transform.position;
            var radius = controller.radius;
            var origin = controller.transform.position + controller.center - 
                         Vector3.up * (controller.height / 2f);
            var layerMask = ~LayerMask.GetMask("Player");
            
            var capsuleGroundCheck = PlayerController.CharacterController.isGrounded;
            var customGroundCheck = Physics.CheckSphere(origin, radius, layerMask);
            
            // Edge detection için daha hassas kontrol
            // var edgeGroundCheck = CheckEdgeSupport(controller, layerMask);
            
            // IsGrounded = (customGroundCheck || capsuleGroundCheck) && edgeGroundCheck;
            IsGrounded = customGroundCheck || capsuleGroundCheck;
            
            // ------------------------------------------------------------------------ //
            
            var rayDistance = controller.radius * 2f;
            var characterBaseWorld = controllerPosition + controller.center - 
                                     Vector3.up * (controller.height / 2f - controller.radius);
            
            var originLeft = characterBaseWorld + (Vector3.left * controller.radius);
            var originRight = characterBaseWorld + (Vector3.right * controller.radius);
            var originFront = characterBaseWorld + (Vector3.forward * controller.radius);
            var originBack = characterBaseWorld + (Vector3.back * controller.radius);
            
            var summedNormals = Vector3.zero;
            var successfulHits = 0;
            RaycastHit hitInfo;

            if (TryGroundNormalCheck(originLeft, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }
            if (TryGroundNormalCheck(originRight, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }
            if (TryGroundNormalCheck(originFront, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }
            if (TryGroundNormalCheck(originBack, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }

            if (successfulHits > 0)
            {
                GroundNormal = (summedNormals / successfulHits).normalized;
            }
            else
            {
                GroundNormal = Vector3.up;
            }
        }

        private bool TryGroundNormalCheck(Vector3 origin, float rayDistance, 
            LayerMask layerMask, out RaycastHit hitInfo)
        {
            var ray = new Ray(origin, Vector3.down);
            var groundCheckHits = new RaycastHit[1];
            
            var numberOfHits = Physics.RaycastNonAlloc(ray, groundCheckHits, 
                rayDistance, layerMask);

            if (numberOfHits > 0)
            {
                // İlk çarpışma bilgisini out parametresine ata
                hitInfo = groundCheckHits[0];

                // Debug için ışınları çiz
                Debug.DrawRay(origin, Vector3.down * hitInfo.distance, Color.green);
                Debug.DrawRay(hitInfo.point, hitInfo.normal * 0.5f, Color.blue);
            
                return true;
            }
            else
            {
                // Çarpışma yoksa, varsayılan RaycastHit değerini ata ve kırmızı ışın çiz
                hitInfo = default; // RaycastHit bir struct olduğu için default değeri atanır
                Debug.DrawRay(origin, Vector3.down * rayDistance, Color.red);
            
                return false;
            }
        }
        
        // ---------------------------------------------------------------------------- //
        
        private void CheckIsFalling()
        {
            // Karakter havada ve aşağı doğru hareket ediyorsa düşüyor demektir
            IsFalling = !IsGrounded && VerticalMovement < 0f;
    
            // Alternatif olarak, CharacterController velocity'sini de kullanabilirsiniz
            // IsFalling = !IsGrounded && CapsulVelocity.y < 0f;
        }
        
        public float CalculateMaxJumpHeight()
        {
            // Yerçekimi çarpanını dahil ederek daha doğru hesaplama
            var effectiveGravity = Gravity * 0.85f;
            var maxHeight = (JumpForce * JumpForce) / (2f * effectiveGravity);
            return maxHeight;
        }

        public Vector3 CalculateMaxJumpPosition()
        {
            var maxHeight = CalculateMaxJumpHeight();
            var currentPosition = PlayerController.transform.position;
            return new Vector3(currentPosition.x, currentPosition.y + maxHeight, currentPosition.z);
        }

        public bool CanJumpToHeight(float targetHeight)
        {
            var maxHeight = CalculateMaxJumpHeight();
            var currentY = PlayerController.transform.position.y;
            return (currentY + maxHeight) >= targetHeight;
        }

        // ---------------------------------------------------------------------------- //
        
        public bool CanRun()
        {
            // Stamina çok düşükse koşamaz
            if (PlayerController.LifeStatsManager.Stamina.Current <= 5f) 
                return false;
    
            // Minimum stamina kontrolü - koşmak için en az 10 stamina gerekli
            var minimumStaminaRequired = 10f;
            return PlayerController.LifeStatsManager.Stamina.Current >= minimumStaminaRequired;

        }

        public bool CanJump()
        {
            // Yerde değilse zıplayamaz
            if (!IsGrounded) return false;

            // Stamina kontrolü - zıplamak için minimum stamina gerekli
            var jumpStaminaCost = StaminaConsumptionCalculator.JumpConsumption(
                PlayerController.LifeStatsManager);
            if (PlayerController.LifeStatsManager.Stamina.Current < jumpStaminaCost)
                return false;
            
            return true;
        }
        
        // ---------------------------------------------------------------------------- //
        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (PlayerController.CharacterController == null) return;
            DrawCharacterControllerGizmo();
            DrawHasObstacleAboveForJumpGizmo();
            DrawCheckIsGroundedGizmo();
            DrawGroundNormalGizmo();
        }

        private void DrawGroundNormalGizmo()
        {
            if (!Application.isPlaying) return;
            
            var rayDistance = _cController.radius * 2f;
            var layerMask = ~LayerMask.GetMask("Player");
            var characterBaseWorld = _cPosition + _cCenter - Vector3.up * 
                (_cHeight / 2f - _cRaius);
            
            // Raycast origin pozisyonları
            var raycastOrigins = new[]
            {
                characterBaseWorld + (Vector3.left * _cRaius),
                characterBaseWorld + (Vector3.right * _cRaius),
                characterBaseWorld + (Vector3.forward * _cRaius),
                characterBaseWorld + (Vector3.back * _cRaius)
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
            
            // Edge detection visualization
#if false
            if (Application.isPlaying)
            {
                var supportRadius = radius * (1f - EdgeDetectionThreshold);
                
                // Merkez kontrol noktası
                Gizmos.color = _isNearEdge ? Color.red : Color.green;
                Gizmos.DrawWireSphere(origin, supportRadius * 0.35f);
                
                // 8 yönlü edge detection points
                for (var i = 0; i < 8; i++)
                {
                    var angle = (360f / 8) * i * Mathf.Deg2Rad;
                    var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    var checkPoint = origin + direction * supportRadius;
                    
                    var layerMask = ~LayerMask.GetMask("Player");
                    var hasSupport = Physics.Raycast(checkPoint, Vector3.down, 
                        radius * 0.5f, layerMask);
                    
                    Gizmos.color = hasSupport ? new Color(0, 1, 0, 0.7f) : 
                        new Color(1, 0, 0, 0.7f);
                    Gizmos.DrawWireSphere(checkPoint, 0.025f);
                    Gizmos.DrawLine(checkPoint, checkPoint + Vector3.down * radius * 0.5f);
                }
            }
#endif
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