using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerGroundDetection : MovementSubBehaviour
    {
        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        
        // Jump buffer - zıplama sonrası kısa süre ground check'i devre dışı bırak
        private float _jumpBuffer;
        private const float JumpBufferTime = 0.2f; // 200ms
        
        // Debug bilgileri için ek özellikler
        private bool _capsuleGroundCheck;
        private bool _customGroundCheck;
        private bool _rayBasedGroundCheck;
        private int _successfulHits;
        
        public PlayerGroundDetection(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController){ }

        public override void Update()
        {
            // Jump buffer'ı azalt
            if (_jumpBuffer > 0)
            {
                _jumpBuffer -= Time.deltaTime;
            }
            
            CheckIsGroundedWithNormal();
        }
        
        // Zıplama başladığında çağrılacak metod
        public void OnJumpStarted()
        {
            _jumpBuffer = JumpBufferTime;
            IsGrounded = false; // Anında grounded durumunu false yap
        }
        
        // ---------------------------------------------------------------------------- //
        
        // Ground check ve normal hesaplamasını birleştiren fonksiyon
        private void CheckIsGroundedWithNormal()
        {
            // Jump buffer aktifse ve karakter yukarı hareket ediyorsa ground check yapma
            if (_jumpBuffer > 0 && MovementBehaviour.VerticalMovement > 0)
            {
                IsGrounded = false;
                GroundNormal = Vector3.up;
                return;
            }
            
            var controller = PlayerController.CharacterController;
            var radius = controller.radius;
            var origin = controller.transform.position + controller.center - Vector3.up * (controller.height / 2f);
            var layerMask = ~LayerMask.GetMask("Player");
            
            // Orijinal ground check'ler
            _capsuleGroundCheck = PlayerController.CharacterController.isGrounded;
            _customGroundCheck = Physics.CheckSphere(origin, radius, layerMask); // Radius biraz küçült
            
            // Ground normal hesaplaması ve ek ground kontrolü
            var (normalCalculated, rayBasedGroundCheck) = CalculateGroundNormalWithGroundCheck();
            _rayBasedGroundCheck = rayBasedGroundCheck;
            
            // Tüm kontrolleri birleştir
            IsGrounded = _capsuleGroundCheck || _customGroundCheck || _rayBasedGroundCheck;
            // IsGrounded = _capsuleGroundCheck;
            // IsGrounded = _customGroundCheck;
            
            // Normal'i ayarla
            GroundNormal = normalCalculated;
        }

        private (Vector3 normal, bool isGrounded) CalculateGroundNormalWithGroundCheck()
        {
            var controller = PlayerController.CharacterController;
            var controllerPosition = controller.transform.position;
            var layerMask = ~LayerMask.GetMask("Player");
            
            // Ray distance'ı daha konservatif yap
            var rayDistance = controller.radius * 1.5f; // 2f'den 1.5f'e düşür
            var characterBaseWorld = controllerPosition + controller.center - Vector3.up * (controller.height / 2f - controller.radius);
            
            // 8 ray origin pozisyonu - radius'u biraz küçült
            var radiusMultiplier = 0.8f; // Radius'un %80'ini kullan
            var origins = new Vector3[]
            {
                // Ana 4 yön
                characterBaseWorld + Vector3.left * (controller.radius * radiusMultiplier),
                characterBaseWorld + Vector3.right * (controller.radius * radiusMultiplier),
                characterBaseWorld + Vector3.forward * (controller.radius * radiusMultiplier),
                characterBaseWorld + Vector3.back * (controller.radius * radiusMultiplier),
                
                // Diagonal 4 yön
                characterBaseWorld + (Vector3.forward + Vector3.left).normalized * (controller.radius * radiusMultiplier),
                characterBaseWorld + (Vector3.forward + Vector3.right).normalized * (controller.radius * radiusMultiplier),
                characterBaseWorld + (Vector3.back + Vector3.left).normalized * (controller.radius * radiusMultiplier),
                characterBaseWorld + (Vector3.back + Vector3.right).normalized * (controller.radius * radiusMultiplier)
            };
            
            var summedNormals = Vector3.zero;
            var successfulHits = 0;
            RaycastHit hitInfo;

            // 8 ray için loop
            foreach (var origin in origins)
            {
                if (TryGroundNormalCheck(origin, rayDistance, layerMask, out hitInfo))
                {
                    summedNormals += hitInfo.normal;
                    successfulHits++;
                }
            }

            _successfulHits = successfulHits; // Debug için sakla

            Vector3 calculatedNormal;
            bool rayBasedGrounded;

            if (successfulHits > 2) // En az 3 ray hit olması gereksin (daha katı kontrol)
            {
                calculatedNormal = (summedNormals / successfulHits).normalized;
                rayBasedGrounded = true;
            }
            else
            {
                calculatedNormal = Vector3.up;
                rayBasedGrounded = false;
            }

            return (calculatedNormal, rayBasedGrounded);
        }

        private bool TryGroundNormalCheck(Vector3 origin, float rayDistance, LayerMask layerMask, out RaycastHit hitInfo)
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

        #region Gizmos

#if UNITY_EDITOR
        public override void OnGUI()
        {
            if (!Application.isPlaying) return;
            DisplayJumpBufferInfo();
        }

        private void DisplayJumpBufferInfo()
        {
            var style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            
            GUI.Label(new Rect(10, 220, 300, 20), $"Jump Buffer: {_jumpBuffer:F2}s", style);
            
            if (_jumpBuffer > 0)
            {
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(10, 240, 300, 20), "Jump Buffer Active - Ground Check Disabled", style);
            }
        }

        public override void OnDrawGizmos()
        {
            DrawCheckIsGroundedGizmo();
            DrawGroundNormalGizmo();
        }
        
        private void DrawCheckIsGroundedGizmo()
        {
            var controller = PlayerController.CharacterController;
            var position = controller.transform.position;
            var center = controller.center;
            var height = controller.height;
            var radius = controller.radius;
            var origin = position + center - Vector3.up * (height / 2f);
            
            // Jump buffer aktifse sarı, değilse normal renk
            if (_jumpBuffer > 0)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
            }
            Gizmos.DrawWireSphere(origin, radius);
        }
        
        private void DrawGroundNormalGizmo()
        {
            if (!Application.isPlaying) return;
            
            var controller = PlayerController.CharacterController;
            var controllerPosition = controller.transform.position;
            var center = controller.center;
            var height = controller.height;
            var radius = controller.radius;
            var rayDistance = controller.radius * 1.5f; // Güncellenmiş ray distance
            var layerMask = ~LayerMask.GetMask("Player");
            var characterBaseWorld = controllerPosition + center - Vector3.up * (height / 2f - radius);
            
            var radiusMultiplier = 0.8f;
            
            // 8 raycast origin pozisyonu
            var raycastOrigins = new[]
            {
                // Ana 4 yön
                characterBaseWorld + (Vector3.left * (radius * radiusMultiplier)),
                characterBaseWorld + (Vector3.right * (radius * radiusMultiplier)),
                characterBaseWorld + (Vector3.forward * (radius * radiusMultiplier)),
                characterBaseWorld + (Vector3.back * (radius * radiusMultiplier)),
                
                // Diagonal 4 yön
                characterBaseWorld + ((Vector3.forward + Vector3.left).normalized * (radius * radiusMultiplier)),
                characterBaseWorld + ((Vector3.forward + Vector3.right).normalized * (radius * radiusMultiplier)),
                characterBaseWorld + ((Vector3.back + Vector3.left).normalized * (radius * radiusMultiplier)),
                characterBaseWorld + ((Vector3.back + Vector3.right).normalized * (radius * radiusMultiplier))
            };
            
            // Origin noktalarını çiz
            Gizmos.color = _jumpBuffer > 0 ? Color.yellow : Color.cyan;
            foreach (var origin in raycastOrigins)
            {
                Gizmos.DrawWireSphere(origin, 0.025f);
            }
            
            // Jump buffer aktifse ray'leri çizme
            if (_jumpBuffer <= 0)
            {
                // Her origin için raycast yap ve sonucu çiz
                foreach (var origin in raycastOrigins)
                {
                    DrawRaycastResult(origin, rayDistance, layerMask);
                }
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
#endif
        
        #endregion
        
        // ---------------------------------------------------------------------------- //
    }
}