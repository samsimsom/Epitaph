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
        
        public bool IsMovementBlocked { get; private set; }
        private Vector3 _lastFramePosition;
        private float _movementThreshold = 0.01f;
        
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
        
        // Prevent Corner Climb Variables
        private float _antiClimbRayHeightOffset = 0.05f;
        private float _antiClimbRayDistance = 0.4f; // 0.6f
        private LayerMask ClimbLayerMask => ~LayerMask.GetMask("Player");

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

        // Edge Detection Variables
        public float EdgeDetectionThreshold = 0.3f; // Yarı çapın yüzdesi (0.3 = %30)
        private bool _isNearEdge = false;

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
        
        private void HandleMovement()
        {
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            
            // Kamera yönüne göre döndür
            var cam = PlayerController.PlayerCamera.transform;
            var forward = Vector3.ProjectOnPlane(cam.forward, 
                Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(cam.right, 
                Vector3.up).normalized;
            moveDirection = moveDirection.z * forward + moveDirection.x * right;
            
            // Köşe tırmanmasını engelle
            // PreventCornerClimb(ref moveDirection);
            
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

        private bool TryGroundNormalCheck(Vector3 origin, float rayDistance, LayerMask layerMask, 
            out RaycastHit hitInfo)
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
        
        private bool CheckEdgeSupport(CharacterController controller, LayerMask layerMask)
        {
            var radius = controller.radius;
            var supportRadius = radius * (1f - EdgeDetectionThreshold); // Daha küçük yarıçap
            var origin = controller.transform.position + controller.center - 
                         Vector3.up * (controller.height / 2f);
            
            // Merkez noktadan daha küçük yarıçapla kontrol et
            var centerSupported = Physics.CheckSphere(origin, 
                supportRadius * 0.5f, layerMask);
            
            if (!centerSupported)
            {
                _isNearEdge = true;
                return false; // Merkez desteklenmiyorsa düş
            }
            
            // 8 yönde edge detection
            var supportedDirections = 0;
            var totalDirections = 8;
            
            for (var i = 0; i < totalDirections; i++)
            {
                var angle = (360f / totalDirections) * i * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var checkPoint = origin + direction * supportRadius;
                
                if (Physics.Raycast(checkPoint, Vector3.down,
                        radius * 0.5f, layerMask))
                {
                    supportedDirections++;
                }
            }
            
            // En az %60'ı desteklenmeli (5/8 yön)
            var supportPercentage = (float)supportedDirections / totalDirections;
            _isNearEdge = supportPercentage < 0.6f;
            
            return supportPercentage >= 0.6f;
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

        private void PreventCornerClimb(ref Vector3 desiredMoveDirection)
        {
            _antiClimbRayHeightOffset = 0.05f;
            _antiClimbRayDistance = 0.4f; // 0.6f
            var controller = PlayerController.CharacterController;
            var position = controller.transform.position;
            var center = controller.center;
            var height = controller.height;
            var stepOffset = controller.stepOffset;
            
            // Işının başlangıç noktası: Karakterin merkezinin biraz altı, stepOffset'in hemen üzeri
            var rayOrigin = position + center - Vector3.up * 
                (height / 2f - (stepOffset + _antiClimbRayHeightOffset));

            // Sadece yatay harekete bakıyoruz
            var horizontalMoveDirection = new Vector3(desiredMoveDirection.x, 0, 
                desiredMoveDirection.z).normalized;

            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, horizontalMoveDirection, out hit, 
                    _antiClimbRayDistance, ClimbLayerMask))
            {
                #region ProjectOnPlane Slide
                #if false
                // ProjectOnPlane ile kaymasını sağlayabiliriz.
                var projectedMove = Vector3.ProjectOnPlane(desiredMoveDirection, hit.normal);
                if (Vector3.Dot(projectedMove, desiredMoveDirection) >= 0) // Geriye doğru itilmesini engelle
                {
                    desiredMoveDirection = projectedMove.normalized * desiredMoveDirection.magnitude;
                }
                else // Çok dik bir açıysa, belki sadece o yöne hareketi kes
                {
                    // Bu kısım daha karmaşık olabilir, en basit çözüm o yöne hareketi kısıtlamak
                    // veya karakteri durdurmak olabilir.
                    // Şimdilik engele doğru olan bileşeni azaltmayı deneyelim:
                    var perpendicularToHit = Vector3.Cross(hit.normal, Vector3.up).normalized;
                    var forwardComponent = Vector3.Dot(desiredMoveDirection, perpendicularToHit);
                    desiredMoveDirection = perpendicularToHit * forwardComponent;
                    Debug.Log($"Dik Aci : {desiredMoveDirection}");
                    
                    // Veya daha basiti, eğer çarptıysa o yönde ilerlemesini kısıtla
                    // desiredMoveDirection = Vector3.zero; // Bu çok ani durdurur.
                }     
                #endif
                #endregion
                
                #region ProjectOnPlane Stop
                #if true
                // Basit bir engelleme için (kaydırmadan):
                // Eğer engel karakterin tam önündeyse ve dik bir yüzeyse, hareketi durdur.
                var angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle > 80 && angle < 100)
                {
                   desiredMoveDirection = Vector3.ProjectOnPlane(desiredMoveDirection, hit.normal);

                   IsMovementBlocked = desiredMoveDirection.sqrMagnitude <= _movementThreshold;
                   // Debug.Log(IsMovementBlocked);
                }
                #endif
                #endregion
            }
        }

        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (PlayerController.CharacterController == null) return;
            DrawCharacterControllerGizmo();
            DrawHasObstacleAboveForJumpGizmo();
            DrawCheckIsGroundedGizmo();
            DrawGroundNormalGizmo();
            // DrawPreventCornerClimb();
            // DrawStepOffsetGizmo();
        }
        
        private void DrawPreventCornerClimb()
        {
            var controller = PlayerController.CharacterController;
            if (controller == null) return;
            
            // PreventCornerClimb methodundaki aynı parametreleri kullan
            _antiClimbRayHeightOffset = 0.05f;
            _antiClimbRayDistance = 0.4f; // 0.6f
            
            // Işının başlangıç noktası: PreventCornerClimb ile aynı
            var position = controller.transform.position;
            var center = controller.center;
            var height = controller.height;
            var stepOffset = controller.stepOffset;
            
            // Işının başlangıç noktası: Karakterin merkezinin biraz altı, stepOffset'in hemen üzeri
            var rayOrigin = position + center - Vector3.up * 
                (height / 2f - (stepOffset + _antiClimbRayHeightOffset));

            // Hareket yönünü almak için mevcut input'u kullan
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            
            // Eğer hareket yoksa, kameranın ileri yönünü kullan
            if (moveDirection.magnitude < 0.1f)
            {
                var cam = PlayerController.PlayerCamera.transform;
                var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                moveDirection = forward;
            }
            else
            {
                // Kamera yönüne göre döndür (HandleMovement ile aynı)
                var cam = PlayerController.PlayerCamera.transform;
                var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                var right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
                moveDirection = moveDirection.z * forward + moveDirection.x * right;
            }
            
            var horizontalMoveDirection = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
            
            // Raycast yap
            var hitDetected = Physics.Raycast(rayOrigin, horizontalMoveDirection, out var hit, 
                _antiClimbRayDistance, ClimbLayerMask);
            
            // Hit varsa kırmızı, yoksa sarı
            Gizmos.color = hitDetected ? Color.red : Color.yellow;
            
            // Gizmo çiz
            Gizmos.DrawWireSphere(rayOrigin, controller.radius * 0.5f);
            
            // Debug ray çiz
            if (hitDetected)
            {
                Gizmos.DrawLine(rayOrigin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
            else
            {
                Gizmos.DrawLine(rayOrigin, rayOrigin + horizontalMoveDirection * _antiClimbRayDistance);
            }
        }

        private void DrawStepOffsetGizmo()
        {
            var controller = PlayerController.CharacterController;
            if (controller == null) return;

            // Character Controller'ın pozisyon ve boyut bilgileri
            var controllerTransform = controller.transform;
            var center = controllerTransform.position + controller.center;
            var radius = controller.radius;
            var stepOffset = controller.stepOffset;

            // Step offset yüksekliğindeki düzlemin konumu
            var stepPlaneY = controllerTransform.position.y + stepOffset;
            var stepPlaneCenter = new Vector3(center.x, stepPlaneY, center.z);

            // Gizmo rengi - step offset aktifken yeşil, değilse gri
            Gizmos.color = stepOffset > 0
                ? new Color(0f, 1f, 0f, 0.7f)
                : new Color(0.5f, 0.5f, 0.5f, 0.7f);

            // Düz plane (disk) şeklinde gizmo çiz
            // Unity'de düz disk çizmek için birden fazla çizgi kullanıyoruz
            var segments = 32;
            var angleStep = 360f / segments;

            for (var i = 0; i < segments; i++)
            {
                var angle1 = i * angleStep * Mathf.Deg2Rad;
                var angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                var point1 = stepPlaneCenter + new Vector3(
                    Mathf.Cos(angle1) * radius,
                    0,
                    Mathf.Sin(angle1) * radius
                );

                var point2 = stepPlaneCenter + new Vector3(
                    Mathf.Cos(angle2) * radius,
                    0,
                    Mathf.Sin(angle2) * radius
                );

                // Dış çember
                Gizmos.DrawLine(point1, point2);

                // Merkezden dışa çizgiler (opsiyonel - daha belirgin görünüm için)
                if (i % 4 == 0) // Her 4. çizgide merkeze bağla
                {
                    Gizmos.DrawLine(stepPlaneCenter, point1);
                }
            }

            // Merkez noktası
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Turuncu
            Gizmos.DrawWireSphere(stepPlaneCenter, 0.05f);

            // Step offset yüksekliğini gösteren dikey çizgi
            Gizmos.color = new Color(1f, 1f, 0f, 0.8f); // Sarı
            var groundLevel =
                new Vector3(center.x, controllerTransform.position.y, center.z);
            Gizmos.DrawLine(groundLevel, stepPlaneCenter);

            // Zemin seviyesi referans çizgisi
            Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.6f); // Koyu gri
            for (var i = 0; i < 8; i++)
            {
                var angle = i * 45f * Mathf.Deg2Rad;
                var point = groundLevel + new Vector3(
                    Mathf.Cos(angle) * radius * 0.8f,
                    0,
                    Mathf.Sin(angle) * radius * 0.8f
                );
                Gizmos.DrawLine(groundLevel, point);
            }
        }

        private void DrawGroundNormalGizmo()
        {
            if (!Application.isPlaying) return;
            
            var controller = PlayerController.CharacterController;
            var controllerPosition = controller.transform.position;
            var rayDistance = controller.radius * 2f;
            var characterBaseWorld = controllerPosition + controller.center - 
                                     Vector3.up * (controller.height / 2f - controller.radius);
            
            var originLeft = characterBaseWorld + (Vector3.left * controller.radius);
            var originRight = characterBaseWorld + (Vector3.right * controller.radius);
            var originFront = characterBaseWorld + (Vector3.forward * controller.radius);
            var originBack = characterBaseWorld + (Vector3.back * controller.radius);
            
            var layerMask = ~LayerMask.GetMask("Player");
            
            // Raycast origin noktalarını çiz
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(originLeft, 0.025f);
            Gizmos.DrawWireSphere(originRight, 0.025f);
            Gizmos.DrawWireSphere(originFront, 0.025f);
            Gizmos.DrawWireSphere(originBack, 0.025f);
            
            // Raycast sonuçlarını çiz
            RaycastHit hitInfo;
            
            if (TryGroundNormalCheck(originLeft, rayDistance, layerMask, out hitInfo))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(originLeft, hitInfo.point);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(originLeft, originLeft + Vector3.down * rayDistance);
            }
            
            if (TryGroundNormalCheck(originRight, rayDistance, layerMask, out hitInfo))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(originRight, hitInfo.point);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(originRight, originRight + Vector3.down * rayDistance);
            }
            
            if (TryGroundNormalCheck(originFront, rayDistance, layerMask, out hitInfo))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(originFront, hitInfo.point);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(originFront, originFront + Vector3.down * rayDistance);
            }
            
            if (TryGroundNormalCheck(originBack, rayDistance, layerMask, out hitInfo))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(originBack, hitInfo.point);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(originBack, originBack + Vector3.down * rayDistance);
            }
            
            // Hesaplanan ortalama ground normal'i çiz
            Gizmos.color = Color.magenta;
            var normalDrawPosition = characterBaseWorld;
            Gizmos.DrawRay(normalDrawPosition, GroundNormal * 1f);
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
            var radius = controller.radius;
            var origin = controller.transform.position + controller.center - 
                         Vector3.up * (controller.height / 2f);
            
            // Ana ground check
            Gizmos.color = IsGrounded ? new Color(0,1,0,0.25f) : 
                new Color(1,0,0,0.25f);
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