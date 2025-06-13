using UnityEditor;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class GroundHandler : MovementSubBehaviour
    {
        private const float RadiusMultiplier = 1.1f;
        private const float RayDistanceMultiplier = 1.5f;
        private const int MinimumHitsForGrounded = 2;

        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        
        // Debug information
        private GroundCheckDebugInfo _debugInfo;
        
        public GroundHandler(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController)
        {
            _debugInfo = new GroundCheckDebugInfo();
        }

        public override void Update()
        {
            var groundCheckResult = PerformGroundCheck();
            
            _debugInfo.UpdateDebugInfo(
                PlayerController.CharacterController.isGrounded,
                groundCheckResult.IsGrounded,
                groundCheckResult.SuccessfulHits
            );
            
            IsGrounded = _debugInfo.CapsuleGroundCheck && groundCheckResult.IsGrounded;
            GroundNormal = groundCheckResult.Normal;
        }
        
        // ---------------------------------------------------------------------------- //
        
        private GroundCheckResult PerformGroundCheck()
        {
            var raycastConfig = CreateRaycastConfiguration();
            var rayOrigins = GenerateRayOrigins(raycastConfig);
            
            return CalculateGroundFromRaycasts(rayOrigins, raycastConfig);
        }

        private RaycastConfiguration CreateRaycastConfiguration()
        {
            var controller = PlayerController.CharacterController;
            
            return new RaycastConfiguration
            {
                Controller = controller,
                ControllerPosition = controller.transform.position,
                LayerMask = ~LayerMask.GetMask("Player"),
                RayDistance = controller.radius * RayDistanceMultiplier,
                CharacterBaseWorld = CalculateCharacterBasePosition(controller),
                EffectiveRadius = controller.radius * RadiusMultiplier
            };
        }

        private Vector3 CalculateCharacterBasePosition(CharacterController controller)
        {
            return controller.transform.position + controller.center - Vector3.up * (controller.height / 2f - controller.radius);
        }

        private Vector3[] GenerateRayOrigins(RaycastConfiguration config)
        {
            var basePosition = config.CharacterBaseWorld;
            var radius = config.EffectiveRadius;
            
            return new[]
            {
                // Cardinal directions
                basePosition + Vector3.left * radius,
                basePosition + Vector3.right * radius,
                basePosition + Vector3.forward * radius,
                basePosition + Vector3.back * radius,
                
                // Diagonal directions
                basePosition + (Vector3.forward + Vector3.left).normalized * radius,
                basePosition + (Vector3.forward + Vector3.right).normalized * radius,
                basePosition + (Vector3.back + Vector3.left).normalized * radius,
                basePosition + (Vector3.back + Vector3.right).normalized * radius
            };
        }

        private GroundCheckResult CalculateGroundFromRaycasts(Vector3[] origins, RaycastConfiguration config)
        {
            var normalSum = Vector3.zero;
            var successfulHits = 0;

            foreach (var origin in origins)
            {
                if (TryRaycastToGround(origin, config, out var hitInfo))
                {
                    normalSum += hitInfo.normal;
                    successfulHits++;
                }
            }

            var isGrounded = successfulHits >= MinimumHitsForGrounded;
            var calculatedNormal = isGrounded ? (normalSum / successfulHits).normalized : Vector3.up;

            return new GroundCheckResult(calculatedNormal, isGrounded, successfulHits);
        }

        private bool TryRaycastToGround(Vector3 origin, RaycastConfiguration config, out RaycastHit hitInfo)
        {
            var ray = new Ray(origin, Vector3.down);
            var hits = new RaycastHit[1];
            
            var hitCount = Physics.RaycastNonAlloc(ray, hits, config.RayDistance, config.LayerMask);

            if (hitCount > 0)
            {
                hitInfo = hits[0];
                return true;
            }

            hitInfo = default;
            return false;
        }
        
        // ---------------------------------------------------------------------------- //

        #region Gizmos

#if UNITY_EDITOR
        public override void OnGUI() 
        {
            if (!Application.isPlaying) return;
            
            // Debug bilgilerini ekranın sol üst köşesinde göster
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Ground Handler Debug Info", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            GUILayout.Label($"Is Grounded: {IsGrounded}");
            GUILayout.Label($"Capsule Ground Check: {_debugInfo.CapsuleGroundCheck}");
            GUILayout.Label($"Ray Based Ground Check: {_debugInfo.RayBasedGroundCheck}");
            GUILayout.Label($"Successful Hits: {_debugInfo.SuccessfulHits}");
            GUILayout.Label($"Ground Normal: {GroundNormal:F2}");
            
            // Renk kodlu durum göstergesi
            var statusColor = IsGrounded ? "green" : "red";
            GUILayout.Label($"<color={statusColor}>Status: {(IsGrounded ? "GROUNDED" : "AIRBORNE")}</color>", 
                new GUIStyle(GUI.skin.label) { richText = true });
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public override void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            DrawGroundCheckVisualization();
        }

        private void DrawGroundCheckVisualization()
        {
            var config = CreateRaycastConfiguration();
            var origins = GenerateRayOrigins(config);
            
            DrawRayOrigins(origins);
            DrawRaycastResults(origins, config);
            DrawCalculatedGroundNormal(config.CharacterBaseWorld);
        }

        private void DrawRayOrigins(Vector3[] origins)
        {
            Gizmos.color = Color.yellow;
            foreach (var origin in origins)
            {
                Gizmos.DrawWireSphere(origin, 0.025f);
            }
        }

        private void DrawRaycastResults(Vector3[] origins, RaycastConfiguration config)
        {
            foreach (var origin in origins)
            {
                if (TryRaycastToGround(origin, config, out var hitInfo))
                {
                    // Hit - green line and blue normal
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(origin, hitInfo.point);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
                }
                else
                {
                    // Miss - red line
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(origin, origin + Vector3.down * config.RayDistance);
                }
            }
        }

        private void DrawCalculatedGroundNormal(Vector3 basePosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(basePosition, GroundNormal * 1f);
        }

#endif
        
        #endregion
    }

    // ---------------------------------------------------------------------------- //
    // Supporting Data Structures
    // ---------------------------------------------------------------------------- //

    public struct RaycastConfiguration
    {
        public CharacterController Controller;
        public Vector3 ControllerPosition;
        public LayerMask LayerMask;
        public float RayDistance;
        public Vector3 CharacterBaseWorld;
        public float EffectiveRadius;
    }

    public struct GroundCheckResult
    {
        public Vector3 Normal;
        public bool IsGrounded;
        public int SuccessfulHits;

        public GroundCheckResult(Vector3 normal, bool isGrounded, int successfulHits)
        {
            Normal = normal;
            IsGrounded = isGrounded;
            SuccessfulHits = successfulHits;
        }
    }

    public class GroundCheckDebugInfo
    {
        public bool CapsuleGroundCheck { get; private set; }
        public bool CustomGroundCheck { get; private set; }
        public bool RayBasedGroundCheck { get; private set; }
        public int SuccessfulHits { get; private set; }

        public void UpdateDebugInfo(bool capsuleCheck, bool rayBasedCheck, int hits)
        {
            CapsuleGroundCheck = capsuleCheck;
            RayBasedGroundCheck = rayBasedCheck;
            SuccessfulHits = hits;
        }
    }
    
}