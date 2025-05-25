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
        public float AirControlFactor = 1.5f;
        public float Gravity = 20.0f;
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
        public BaseState CurrentState { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        
        public Vector3 CapsulVelocity { get; set; }
        public float CurrentSpeed { get; set; }
        public float VerticalMovement { get; set; }

        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }

        #region Gizmo & GUI Variables

        private GUIStyle _myStyle;

        #endregion

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
            CurrentState.UpdateState();
            HandleMovement();
            
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
            
            // Dikey hızı hareket vektörüne uygula
            moveDirection.y = VerticalMovement;

            // Hareket uygula
            PlayerController.CharacterController.Move(moveDirection * Time.deltaTime);
            
            // Update CurrentVelocity
            CapsulVelocity = PlayerController.CharacterController.velocity;
            CurrentSpeed = CapsulVelocity.magnitude;
        }

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
            var radius = controller.radius;
            var origin = controller.transform.position + controller.center - 
                         Vector3.up * (controller.height / 2f);
            var layerMask = ~LayerMask.GetMask("Player");
            
            var capsuleGroundCheck = PlayerController.CharacterController.isGrounded;
            var customGroundCheck = Physics.CheckSphere(origin, radius, layerMask);
            IsGrounded = customGroundCheck || capsuleGroundCheck;
            
            // ------------------------------------------------------------------------ //
            
            var rayDistance = controller.radius;
            var characterBaseWorld = controller.transform.position + controller.center - Vector3.up * (controller.height / 2f);
            
            var originLeft = characterBaseWorld + (Vector3.left * controller.radius);
            var originRight = characterBaseWorld + (Vector3.right * controller.radius);
            var originFront = characterBaseWorld + (Vector3.forward * controller.radius);
            var originBack = characterBaseWorld + (Vector3.back * controller.radius);
            
            var summedNormals = Vector3.zero;
            var successfulHits = 0;
            RaycastHit hitInfo;

            if (TryGroundCheck(originLeft, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }
            if (TryGroundCheck(originRight, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }
            if (TryGroundCheck(originFront, rayDistance, layerMask, out hitInfo))
            {
                summedNormals += hitInfo.normal;
                successfulHits++;
            }
            if (TryGroundCheck(originBack, rayDistance, layerMask, out hitInfo))
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
                // Eğer hiçbir ışın yere değmiyorsa (veya IsCustomGrounded false ise),
                // varsayılan olarak dikey normal kullanın.
                GroundNormal = Vector3.up;
            }

        }

        private bool TryGroundCheck(Vector3 origin, float rayDistance, LayerMask layerMask, 
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
        
        // ---------------------------------------------------------------------------- //
        
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (PlayerController.CharacterController == null) return;
            DrawCharacterControllerGizmo();
            DrawHasObstacleAboveForJumpGizmo();
            DrawCheckIsGroundedGizmo();
        }

        public override void OnGUI()
        {
            if (_myStyle == null)
            {
                _myStyle = new GUIStyle();
                _myStyle.fontSize = 18;
                _myStyle.normal.textColor = Color.white;
            }

            GUI.Label(new Rect(10, 10, 300, 20), 
                $"Vertical Movement : {VerticalMovement:F1}", _myStyle);
            GUI.Label(new Rect(10, 30, 300, 20), 
                $"Capsul Velocity : {CapsulVelocity}", _myStyle);
            GUI.Label(new Rect(10, 50, 300, 20), 
                $"Current Movement : {CurrentSpeed:F1}", _myStyle);
            
            GUI.Label(new Rect(10, 70, 300, 20), 
                $"Is Grounded Custom: {IsGrounded}", _myStyle);
            GUI.Label(new Rect(10, 90, 300, 20), 
                $"Is Grounded Capsule: {PlayerController.CharacterController.isGrounded}", _myStyle);
            
            GUI.Label(new Rect(10, 110, 300, 20), 
                $"Ground Normal : {GroundNormal}", _myStyle);
            
            GUI.Label(new Rect(10, 130, 300, 20), 
                $"Movement State : {CurrentState.StateName}", _myStyle);
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

        private void DrawCheckIsGroundedGizmo()
        {
            var controller = PlayerController.CharacterController;
            var radius = controller.radius;
            var origin = controller.transform.position + controller.center - 
                         Vector3.up * (controller.height / 2f);
            
            Gizmos.color = IsGrounded ? new Color(0,1,0,0.25f) : 
                new Color(1,0,0,0.25f);
            Gizmos.DrawWireSphere(origin, radius);
        }
#endif

    }
}