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

        // Jump Variables
        public float JumpForce = 5.0f;
        public float AirControlFactor = 1.25f;
        public float Gravity = 20.0f;
        public float CoyoteTime = 0.2f; // Saniye cinsinden coyote süresi
        public float CoyoteTimeCounter;

        public float TerminalVelocity = -20.0f;
        private float _verticalVelocity;

        // Crouch Variables
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);
        public float NormalCameraHeight = 1.5f;
        public float CrouchCameraHeight = 0.7f;
        public float CrouchTransitionDuration = 0.2f;
        
        // Input Variables
        private Vector2 _currentMovementInput;

        // Getters & Setters
        public BaseState CurrentState { get; set; }
        public bool IsCrouching { get; set; }
        public Vector3 CurrentVelocity { get; set; }
        public float CurrentMovementY
        {
            get => _verticalVelocity;
            set => _verticalVelocity = value;
        }
        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }

        // Sınıfın değişkenler bölümüne ekleyin
        public bool IsCustomGrounded { get; private set; }
        public float GroundCheckDistance = 0.2f;

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
        
        // Update metodunda çağırın
        public override void Update()
        {
            Debug.Log(IsCustomGrounded);
            CheckCustomGrounded();
            CurrentState.UpdateState();
            HandleMovement();
            HandleGravity();
            
            // Coyote time yönetimi
            if (IsCustomGrounded)
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
            CurrentState.FixedUpdateState();
        }

        // ---------------------------------------------------------------------------- //

        private void HandleMovement()
        {
            // XZ düzleminde hareket (normalize ile hızlı yön değişimlerinde hız kaybı engellenir)
            var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
            
            // Kamera yönüne göre döndür
            var cam = PlayerController.PlayerCamera.transform;
            var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            moveDirection = moveDirection.z * forward + moveDirection.x * right;

            // Zeminin normalini bul ve hareket yönünü buna göre ayarla (eğimde zıplama sorununu çözer)
            if (IsCustomGrounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(PlayerController.CharacterController.transform.position, Vector3.down, out hit, 1.5f))
                {
                    // Hareket vektörünü zemin normaline göre ayarla
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal).normalized * moveDirection.magnitude;
                }
            }

            // Dikey hızı hareket vektörüne uygula
            moveDirection.y = _verticalVelocity;

            // Hareket uygula
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);

            // Yere değerse dikey hızı hafifçe sabitle, böylece character controller'ın
            // "yerde kayma bugı" azalır
            if (IsCustomGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -1f;
            }
            
            CurrentVelocity = PlayerController.CharacterController.velocity;
        }

        private void HandleGravity()
        {
            if (IsCustomGrounded)
            {
                // Zemin normal açısını kontrol et
                RaycastHit hit;
                var origin = PlayerController.CharacterController.transform.position +
                            PlayerController.CharacterController.center -
                            Vector3.up * (PlayerController.CharacterController.height / 2f);
        
                if (Physics.Raycast(origin, Vector3.down, out hit, 0.5f))
                {
                    var groundAngle = Vector3.Angle(hit.normal, Vector3.up);
                    var slopeLimit = PlayerController.CharacterController.slopeLimit;
            
                    // Eğer eğim varsa ve geçerli bir eğimse, eğime göre yerçekimi uygula
                    if (groundAngle > 0 && groundAngle <= slopeLimit)
                    {
                        // Eğim yönünde küçük bir kuvvet uygula (eğimde aşağı kayma efekti)
                        var slopeForce = 2.0f; // Bu değeri eğimde kayma hızına göre ayarlayabilirsiniz
                        _verticalVelocity = -slopeForce * (groundAngle / slopeLimit);
                        return;
                    }
                }
        
                // Eğer yere yeni değildiyse ve aşağı
                // yönde hareket ediyorsak hızı sıfırla
                if (_verticalVelocity < 0)
                    _verticalVelocity = -0.1f;
            }
            else
            {
                // Daha kontrollü bir düşüş eğrisi için gravity factor'u
                var gravityMultiplier = 1.0f;
                // Eğer jump tusu bırakıldıysa veya oyuncu alçalmaya başladıysa
                // gravity hızlanabilir
                if (_verticalVelocity < 0)
                    gravityMultiplier = 1.5f; // Daha gerçekçi düşüş için
                _verticalVelocity -= Gravity * gravityMultiplier * Time.deltaTime;
        
                // Terminal velocity uygulanıyor
                _verticalVelocity = Mathf.Max(_verticalVelocity, TerminalVelocity);
            }
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
            // var castEnd = castStart + Vector3.up * checkDistance;

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
        
        public bool CanJumpOnCurrentGround()
        {
            // CharacterController'ın slopeLimit'i degree cinsinden
            var slopeLimit = PlayerController.CharacterController.slopeLimit;
    
            // Ayak noktasından aşağıya doğru kısa bir ray at
            RaycastHit hit;
            var controller = PlayerController.CharacterController;
            var origin = controller.transform.position + 
                         controller.center - 
                         Vector3.up * (controller.height / 2f);

            if (Physics.Raycast(origin, Vector3.down, out hit, 0.5f))
            {
                // Yüzey normali ile yukarı vektörü arasındaki açıyı bul
                var groundAngle = Vector3.Angle(hit.normal, Vector3.up);
                // Eğer eğim slopeLimit'e eşit veya daha dikse, zıplayamaz
                return groundAngle <= slopeLimit;
            }
            
            // Raycast boşluğa denk geldiyse, zıplamaya izin ver
            return true;
        }
        
        // ---------------------------------------------------------------------------- //
        
        private void CheckCustomGrounded()
        {
            // CharacterController pozisyonundan aşağıya doğru raycast at
            var controller = PlayerController.CharacterController;
            var origin = controller.transform.position + controller.center - Vector3.up * (controller.height / 2f - 0.1f);
    
            // Çoklu raycast ile daha hassas kontrol
            var hitGround = false;
    
            // Merkez raycast
            if (Physics.Raycast(origin, Vector3.down, out var hit, GroundCheckDistance))
            {
                hitGround = true;
            }
    
            // İleri raycast
            if (!hitGround)
            {
                var forwardOrigin = origin + controller.transform.forward * (controller.radius * 0.8f);
                if (Physics.Raycast(forwardOrigin, Vector3.down, GroundCheckDistance))
                {
                    hitGround = true;
                }
            }
    
            // Geri raycast
            if (!hitGround)
            {
                var backwardOrigin = origin - controller.transform.forward * (controller.radius * 0.8f);
                if (Physics.Raycast(backwardOrigin, Vector3.down, GroundCheckDistance))
                {
                    hitGround = true;
                }
            }
    
            // Sağ raycast
            if (!hitGround)
            {
                var rightOrigin = origin + controller.transform.right * (controller.radius * 0.8f);
                if (Physics.Raycast(rightOrigin, Vector3.down, GroundCheckDistance))
                {
                    hitGround = true;
                }
            }
    
            // Sol raycast
            if (!hitGround)
            {
                var leftOrigin = origin - controller.transform.right * (controller.radius * 0.8f);
                if (Physics.Raycast(leftOrigin, Vector3.down, GroundCheckDistance))
                {
                    hitGround = true;
                }
            }
    
            IsCustomGrounded = hitGround;
        }
        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (PlayerController.CharacterController == null) return;
            DrawCharacterControllerGizmo();
            DrawHasObstacleAboveForJumpGizmo();
            DrawGroundAngleGizmo();
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

        private void DrawGroundAngleGizmo()
        {
            var controller = PlayerController.CharacterController;
            var origin = controller.transform.position + 
                         controller.center - 
                         Vector3.up * (controller.height / 2f);
            
            var radius = PlayerController.CharacterController.radius;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, radius);
        }
#endif

    }
}