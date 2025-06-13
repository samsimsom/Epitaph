using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player;
using Epitaph.Scripts.Player.MovementSystem;
using UnityEngine;

namespace Epitaph.Scripts.DebugPanel
{
    public class DebugLabel : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;

        private GUIStyle _labelStyle;
        private float _currentYPosition; // Dinamik Y pozisyonu için eklendi
        
        private void Awake()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 20; // Yazı tipi boyutu biraz küçültüldü, isteğe bağlı
            _labelStyle.normal.textColor = Color.white;
        }

        // Yardımcı metot GUI.Label çizmek için
        private void DrawGuiLabel(string text)
        {
            GUI.Label(new Rect(10, _currentYPosition, 300, 20), text, _labelStyle);
            _currentYPosition += 25; // Bir sonraki label için Y pozisyonunu artır (20 yükseklik + 2 boşluk)
        }

        private void OnGUI()
        {
            // playerController veya gerekli alt bileşenler null ise OnGUI'yi çalıştırma
            if (playerController == null || playerController.MovementBehaviour == null || 
                playerController.CharacterController == null || GameTime.Instance == null)
            {
                GUI.Label(new Rect(10, 10, 300, 20), "PlayerController or components not ready.", _labelStyle);
                return;
            }

            _currentYPosition = 10f; // Her OnGUI çağrısında Y pozisyonunu sıfırla

            DrawGuiLabel($"Clock : {GameTime.Instance.GameHour:D2}:{GameTime.Instance.GameMinute:D2}");
            
            _currentYPosition += 5f; // Bölümler arası küçük boşluk
            
            DrawGuiLabel($"Is Walking : {playerController.MovementBehaviour.LocomotionHandler.IsWalking}");
            DrawGuiLabel($"Is Running : {playerController.MovementBehaviour.LocomotionHandler.IsRunning}");
            DrawGuiLabel($"Is Jumping : {playerController.MovementBehaviour.JumpHandler.IsJumping}");
            DrawGuiLabel($"Is Falling : {playerController.MovementBehaviour.FallHandler.IsFalling}");
            
            DrawGuiLabel($"Is Crouching : {playerController.MovementBehaviour.CrouchHandler.IsCrouching}");
            DrawGuiLabel($"Is Grounded (Custom): {playerController.MovementBehaviour.GroundHandler.IsGrounded}");
            DrawGuiLabel($"Is Grounded (Capsule): {playerController.CharacterController.isGrounded}");
            
            _currentYPosition += 5f; // Bölümler arası küçük boşluk
            
            DrawGuiLabel($"Vertical Movement : {playerController.MovementBehaviour.GravityHandler.VerticalMovement:F1}");
            DrawGuiLabel($"Capsule Velocity : {playerController.MovementBehaviour.LocomotionHandler.CapsulVelocity}");
            DrawGuiLabel($"Current Movement Speed: {playerController.MovementBehaviour.LocomotionHandler.CurrentSpeed:F1}");

            DrawGuiLabel($"Ground Normal : {playerController.MovementBehaviour.GroundHandler.GroundNormal}");
            DrawGuiLabel($"Movement State : {playerController.MovementBehaviour.StateManager.CurrentState?.StateName ?? "N/A"}");

#if true
            _currentYPosition += 10f; // Life stats öncesi daha büyük boşluk
            
            // DrawGuiLabel($"Can Run: {playerController.MovementBehaviour.CanRun()}");
            // DrawGuiLabel($"Can Jump: {playerController.MovementBehaviour.CanJump()}");
#endif
            
            
        }
    }
}