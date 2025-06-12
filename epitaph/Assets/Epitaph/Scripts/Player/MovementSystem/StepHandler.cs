using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class StepHandler : MovementSubBehaviour
    {
        public float RayDistanceOffset { get; set; } = 0.1f;
        public float RaySpreadAngle { get; set; } = 35.0f;
        public float MinStepOffset { get; set; } = 0.05f;
        
        private readonly float _stepOffset;
        
        // RaycastNonAlloc için buffer'lar
        private readonly RaycastHit[] _raycastBuffer = new RaycastHit[1];
        
        // Cache the last raycast data for gizmo drawing
        private RaycastData _lastRaycastData;
        private RaycastResults _lastRaycastResults;
        private bool _hasValidData;

        public StepHandler(MovementBehaviour movementBehaviour, PlayerController playerController) 
            : base(movementBehaviour, playerController)
        {
            _stepOffset = PlayerController.CharacterController.stepOffset;
        }

        public void HandleStepOffset(Vector2 moveInput)
        {
            var moveDirection = GetNormalizedMoveDirection(moveInput);
            if (moveDirection == Vector3.zero)
            {
                PlayerController.CharacterController.stepOffset = MinStepOffset;
                _hasValidData = false;
                return;
            }

            var raycastData = CalculateRaycastData(moveDirection);
            var raycastResults = PerformStepDetectionRaycasts(raycastData);
            
            // Cache data for gizmo drawing
            _lastRaycastData = raycastData;
            _lastRaycastResults = raycastResults;
            _hasValidData = true;
            
            UpdateStepOffset(raycastResults);
        }

        private Vector3 GetNormalizedMoveDirection(Vector2 moveInput)
        {
            var moveDirXZ = new Vector3(moveInput.x, 0, moveInput.y);
            // Kamera yönüne göre döndür
            var cam = PlayerController.PlayerCamera.transform;
            var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            var right = Vector3.ProjectOnPlane(cam.right, Vector3.up);
            moveDirXZ = moveDirXZ.z * forward + moveDirXZ.x * right;
            return moveDirXZ.normalized;
        }

        private RaycastData CalculateRaycastData(Vector3 moveDirection)
        {
            var controller = PlayerController.CharacterController;
            var distance = controller.radius + controller.skinWidth + RayDistanceOffset;
            
            var bottom = controller.transform.position - new Vector3(0f, controller.height / 2f - controller.center.y, 0f);
            var bottomWithStepOffset = new Vector3(bottom.x, bottom.y + _stepOffset, bottom.z);
            
            // 35 derece sağ ve sol yönler
            var rightDirection = Quaternion.Euler(0, RaySpreadAngle, 0) * moveDirection;
            var leftDirection = Quaternion.Euler(0, -RaySpreadAngle, 0) * moveDirection;
            
            return new RaycastData
            {
                MoveDirection = moveDirection,
                RightDirection = rightDirection,
                LeftDirection = leftDirection,
                Distance = distance,
                BottomPosition = bottom,
                StepOffsetPosition = bottomWithStepOffset
            };
        }

        private RaycastResults PerformStepDetectionRaycasts(RaycastData data)
        {
            // Ana yön raycast'ları
            var bottomHitCount = Physics.RaycastNonAlloc(data.BottomPosition, data.MoveDirection, _raycastBuffer, data.Distance);
            var stepOffsetHitCount = Physics.RaycastNonAlloc(data.StepOffsetPosition, data.MoveDirection, _raycastBuffer, data.Distance);
            
            // Sağ yön raycast'ları (45 derece)
            var bottomRightHitCount = Physics.RaycastNonAlloc(data.BottomPosition, data.RightDirection, _raycastBuffer, data.Distance);
            var stepOffsetRightHitCount = Physics.RaycastNonAlloc(data.StepOffsetPosition, data.RightDirection, _raycastBuffer, data.Distance);
            
            // Sol yön raycast'ları (45 derece)
            var bottomLeftHitCount = Physics.RaycastNonAlloc(data.BottomPosition, data.LeftDirection, _raycastBuffer, data.Distance);
            var stepOffsetLeftHitCount = Physics.RaycastNonAlloc(data.StepOffsetPosition, data.LeftDirection, _raycastBuffer, data.Distance);
            
            // Hit count'ları boolean'a çevir
            var centerBottomHit = bottomHitCount > 0;
            var centerStepOffsetHit = stepOffsetHitCount > 0;
            var rightBottomHit = bottomRightHitCount > 0;
            var rightStepOffsetHit = stepOffsetRightHitCount > 0;
            var leftBottomHit = bottomLeftHitCount > 0;
            var leftStepOffsetHit = stepOffsetLeftHitCount > 0;
            
            // Tüm raycast'ların ortalamasını hesapla
            var totalBottomHits = (centerBottomHit ? 1 : 0) + (rightBottomHit ? 1 : 0) + (leftBottomHit ? 1 : 0);
            var totalStepOffsetHits = (centerStepOffsetHit ? 1 : 0) + (rightStepOffsetHit ? 1 : 0) + (leftStepOffsetHit ? 1 : 0);
            
            // Çoğunluk kuralı: 3 raycast'tan en az 1'si hit olmalı
            var averageBottomHit = totalBottomHits >= 1;
            var averageStepOffsetHit = totalStepOffsetHits >= 1;
            
            return new RaycastResults
            {
                BottomHit = averageBottomHit,
                StepOffsetHit = averageStepOffsetHit,
                
                // Debug için individual hit bilgileri
                CenterBottomHit = centerBottomHit,
                CenterStepOffsetHit = centerStepOffsetHit,
                RightBottomHit = rightBottomHit,
                RightStepOffsetHit = rightStepOffsetHit,
                LeftBottomHit = leftBottomHit,
                LeftStepOffsetHit = leftStepOffsetHit
            };
        }

        private void UpdateStepOffset(RaycastResults results)
        {
            var newStepOffset = DetermineStepOffset(results);
            PlayerController.CharacterController.stepOffset = newStepOffset;
        }

        private float DetermineStepOffset(RaycastResults results)
        {
            return (results.BottomHit, results.StepOffsetHit) switch
            {
                (true, true) => MinStepOffset,  // Wall detected - block stepping
                (true, false) => _stepOffset,   // Step detected - allow stepping
                _ => MinStepOffset                                  // No obstacle or clear path - block stepping
            };
        }

        // Helper structs for better data organization
        private struct RaycastData
        {
            public Vector3 MoveDirection;
            public Vector3 RightDirection;
            public Vector3 LeftDirection;
            public float Distance;
            public Vector3 BottomPosition;
            public Vector3 StepOffsetPosition;
        }

        private struct RaycastResults
        {
            public bool BottomHit;
            public bool StepOffsetHit;
            
            // Debug için individual hit bilgileri
            public bool CenterBottomHit;
            public bool CenterStepOffsetHit;
            public bool RightBottomHit;
            public bool RightStepOffsetHit;
            public bool LeftBottomHit;
            public bool LeftStepOffsetHit;
        }
        
        // ---------------------------------------------------------------------------- //

        #region Gizmos

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            DrawCharacterControllerGizmo();
            
            if (_hasValidData)
            {
                DrawDebugVisualization(_lastRaycastData, _lastRaycastResults);
            }
        }
        
        private void DrawDebugVisualization(RaycastData data, RaycastResults results)
        {
            // Ana yön raycast visualization
            Gizmos.color = results.CenterBottomHit ? Color.red : Color.green;
            Gizmos.DrawRay(data.BottomPosition, data.MoveDirection * data.Distance);
            
            Gizmos.color = results.CenterStepOffsetHit ? Color.blue : Color.cyan;
            Gizmos.DrawRay(data.StepOffsetPosition, data.MoveDirection * data.Distance);
            
            // Sağ yön raycast visualization (45 derece)
            Gizmos.color = results.RightBottomHit ? Color.red : Color.green;
            Gizmos.DrawRay(data.BottomPosition, data.RightDirection * data.Distance);
            
            Gizmos.color = results.RightStepOffsetHit ? Color.blue : Color.cyan;
            Gizmos.DrawRay(data.StepOffsetPosition, data.RightDirection * data.Distance);
            
            // Sol yön raycast visualization (45 derece)
            Gizmos.color = results.LeftBottomHit ? Color.red : Color.green;
            Gizmos.DrawRay(data.BottomPosition, data.LeftDirection * data.Distance);
            
            Gizmos.color = results.LeftStepOffsetHit ? Color.blue : Color.cyan;
            Gizmos.DrawRay(data.StepOffsetPosition, data.LeftDirection * data.Distance);
            
            // Position markers
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(data.BottomPosition, Vector3.up * 0.1f);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(data.StepOffsetPosition, Vector3.up * 0.1f);
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
#endif
      
        // ---------------------------------------------------------------------------- //
        
#if UNITY_EDITOR
        public override void OnGUI()
        {
            if (!Application.isPlaying) return;

            // DisplayStepDetectionInfo();
        }

        private void DisplayStepDetectionInfo()
        {
            var stepDetectionStyle = new GUIStyle();
            stepDetectionStyle.fontSize = 14;
            stepDetectionStyle.normal.textColor = Color.white;
            
            var yOffset = 275f; // PlayerGroundDetection'dan sonra göstermek için offset
            var lineHeight = 20f;
            
            // Step Detection başlığı
            stepDetectionStyle.normal.textColor = Color.white;
            stepDetectionStyle.fontSize = 16;
            GUI.Label(new Rect(10, yOffset, 300, lineHeight), "Step Detection:", stepDetectionStyle);
            yOffset += lineHeight + 5f;
            
            stepDetectionStyle.fontSize = 14;
            
            if (!_hasValidData)
            {
                stepDetectionStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(20, yOffset, 300, lineHeight), "• No movement data", stepDetectionStyle);
                return;
            }
            
            // Mevcut step offset bilgisi
            stepDetectionStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), $"• Current Step Offset: {PlayerController.CharacterController.stepOffset:F3}", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), $"• Max Step Offset: {_stepOffset:F3}", stepDetectionStyle);
            yOffset += lineHeight;
            
            // Raycast sonuçları başlığı
            stepDetectionStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, yOffset, 300, lineHeight), "Raycast Results:", stepDetectionStyle);
            yOffset += lineHeight;
            
            // Center raycast'ları
            stepDetectionStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), "Center Direction:", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.CenterBottomHit ? Color.red : Color.green;
            GUI.Label(new Rect(30, yOffset, 300, lineHeight), $"• Bottom Hit: {_lastRaycastResults.CenterBottomHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.CenterStepOffsetHit ? Color.red : Color.green;
            GUI.Label(new Rect(30, yOffset, 300, lineHeight), $"• Step Offset Hit: {_lastRaycastResults.CenterStepOffsetHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            // Right raycast'ları
            stepDetectionStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), "Right Direction:", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.RightBottomHit ? Color.red : Color.green;
            GUI.Label(new Rect(30, yOffset, 300, lineHeight), $"• Bottom Hit: {_lastRaycastResults.RightBottomHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.RightStepOffsetHit ? Color.red : Color.green;
            GUI.Label(new Rect(30, yOffset, 300, lineHeight), $"• Step Offset Hit: {_lastRaycastResults.RightStepOffsetHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            // Left raycast'ları
            stepDetectionStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), "Left Direction:", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.LeftBottomHit ? Color.red : Color.green;
            GUI.Label(new Rect(30, yOffset, 300, lineHeight), $"• Bottom Hit: {_lastRaycastResults.LeftBottomHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.LeftStepOffsetHit ? Color.red : Color.green;
            GUI.Label(new Rect(30, yOffset, 300, lineHeight), $"• Step Offset Hit: {_lastRaycastResults.LeftStepOffsetHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            // Ortalama sonuçlar
            yOffset += 5f;
            stepDetectionStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, yOffset, 300, lineHeight), "Average Results:", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.BottomHit ? Color.red : Color.green;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), $"• Bottom Hit (Average): {_lastRaycastResults.BottomHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            stepDetectionStyle.normal.textColor = _lastRaycastResults.StepOffsetHit ? Color.red : Color.green;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), $"• Step Offset Hit (Average): {_lastRaycastResults.StepOffsetHit}", stepDetectionStyle);
            yOffset += lineHeight;
            
            // Step durumu
            yOffset += 5f;
            stepDetectionStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, yOffset, 300, lineHeight), "Step Status:", stepDetectionStyle);
            yOffset += lineHeight;
            
            var stepStatus = DetermineStepStatus(_lastRaycastResults);
            var stepColor = stepStatus switch
            {
                "Wall Detected" => Color.red,
                "Step Detected" => Color.yellow,
                "Clear Path" => Color.green,
                _ => Color.white
            };
            
            stepDetectionStyle.normal.textColor = stepColor;
            GUI.Label(new Rect(20, yOffset, 300, lineHeight), $"• Status: {stepStatus}", stepDetectionStyle);
        }

        private string DetermineStepStatus(RaycastResults results)
        {
            return (results.BottomHit, results.StepOffsetHit) switch
            {
                (true, true) => "Wall Detected",
                (true, false) => "Step Detected", 
                _ => "Clear Path"
            };
        }
#endif

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}