using System;
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

        // Player Life Stats
        private string _healthText;
        private float _healthOldValue;
        private float _healthNewValue;
        
        private string _vitalityText;
        private float _vitalityOldValue;
        private float _vitalityNewValue;
        
        private string _staminaText;
        private float _staminaOldValue;
        private float _staminaNewValue;
        
        private string _fatiqueText;
        private float _fatiqueOldValue;
        private float _fatiqueNewValue;
        
        private string _thirstText;
        private float _thirstOldValue;
        private float _thirstNewValue;
        
        private string _hungerText;
        private float _hungerOldValue;
        private float _hungerNewValue;
        
        private string _temperatureText;
        private float _temperatureOldValue;
        private float _temperatureNewValue;
        
        private void Awake()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 20; // Yazı tipi boyutu biraz küçültüldü, isteğe bağlı
            _labelStyle.normal.textColor = Color.white;
        }

        private void Start()
        {
            if (playerController == null || playerController.LifeStatsManager == null)
            {
                Debug.LogError("PlayerController or LifeStatsManager not assigned or initialized in DebugLabel.");
                enabled = false; // Bileşeni devre dışı bırak
                return;
            }
            playerController.LifeStatsManager.OnStatChanged += OnLifeStatsManagerOnOnStatChanged;
            playerController.LifeStatsManager.OnStatCritical += OnLifeStatsManagerOnOnStatCritical;
            playerController.LifeStatsManager.OnDeath += OnLifeStatsManagerOnOnDeath;
        }

        private void OnDisable()
        {
            if (playerController != null && playerController.LifeStatsManager != null)
            {
                playerController.LifeStatsManager.OnStatChanged -= OnLifeStatsManagerOnOnStatChanged;
                playerController.LifeStatsManager.OnStatCritical -= OnLifeStatsManagerOnOnStatCritical;
                playerController.LifeStatsManager.OnDeath -= OnLifeStatsManagerOnOnDeath;
            }
        }

        private void OnLifeStatsManagerOnOnDeath()
        {
            Debug.Log("Character is dead!");
        }

        private void OnLifeStatsManagerOnOnStatCritical(string stat, float val)
        {
            // Debug.Log($"{stat} is now CRITICAL: {val}");
        }

        private void OnLifeStatsManagerOnOnStatChanged(string stat, float cur, float old)
        {
            // Debug.Log($"{stat} changed: {old:F1} -> {cur:F1}"); // Loglamada da formatlama eklenebilir
            
            switch (stat)
            {
                case "Health":
                    _healthText = $"Health";
                    _healthOldValue = old;
                    _healthNewValue = cur;
                    break;
                case "Vitality":
                    _vitalityText = $"Vitality";
                    _vitalityOldValue = old;
                    _vitalityNewValue = cur;
                    break;
                case "Stamina":
                    _staminaText = $"Stamina";
                    _staminaOldValue = old;
                    _staminaNewValue = cur;
                    break;
                case "Fatique":
                    _fatiqueText = $"Fatique";
                    _fatiqueOldValue = old;
                    _fatiqueNewValue = cur;
                    break;
                case "Thirst":
                    _thirstText = $"Thirst";
                    _thirstOldValue = old;
                    _thirstNewValue = cur;
                    break;
                case "Hunger":
                    _hungerText = $"Hunger";
                    _hungerOldValue = old;
                    _hungerNewValue = cur;
                    break;
                case "Temperature":
                    _temperatureText = $"Temperature";
                    _temperatureOldValue = old;
                    _temperatureNewValue = cur;
                    break;
                default:
                    // throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown stat type."); // Daha bilgilendirici hata
                    Debug.LogWarning($"Unknown stat type changed: {stat}");
                    break;
            }
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
            
            DrawGuiLabel($"Is Walking : {playerController.MovementBehaviour.IsWalking}");
            DrawGuiLabel($"Is Running : {playerController.MovementBehaviour.IsRunning}");
            DrawGuiLabel($"Is Jumping : {playerController.MovementBehaviour.IsJumping}");
            DrawGuiLabel($"Is Falling : {playerController.MovementBehaviour.IsFalling}");
            
            DrawGuiLabel($"Is Crouching : {playerController.MovementBehaviour.IsCrouching}");
            DrawGuiLabel($"Is Grounded (Custom): {playerController.MovementBehaviour.IsGrounded}");
            DrawGuiLabel($"Is Grounded (Capsule): {playerController.CharacterController.isGrounded}");
            
            _currentYPosition += 5f; // Bölümler arası küçük boşluk
            
            DrawGuiLabel($"Vertical Movement : {playerController.MovementBehaviour.VerticalMovement:F1}");
            DrawGuiLabel($"Capsule Velocity : {playerController.MovementBehaviour.CapsulVelocity}");
            DrawGuiLabel($"Current Movement Speed: {playerController.MovementBehaviour.CurrentSpeed:F1}");

            DrawGuiLabel($"Ground Normal : {playerController.MovementBehaviour.GroundNormal}");
            DrawGuiLabel($"Movement State : {playerController.MovementBehaviour.CurrentState?.StateName ?? "N/A"}");

#if true
            _currentYPosition += 10f; // Life stats öncesi daha büyük boşluk
            
            // Değişen Life Stats bilgilerini göster
            DrawGuiLabel($"Health : {playerController.LifeStatsManager.Health.Current:F}");
            DrawGuiLabel($"Vitality Ratio : {playerController.LifeStatsManager.VitalityRatio:F}");
            DrawGuiLabel($"Vitality : {playerController.LifeStatsManager.Vitality.Current:F}");
            
            _currentYPosition += 5f; // Bölümler arası küçük boşluk
            
            // DrawGuiLabel($"Stamina : {playerController.LifeStatsManager.Stamina.Current:F}");
            DrawGuiLabel($"Current Stamina: {playerController.LifeStatsManager.Stamina.Current:F1}");
            var consumption = StaminaConsumptionCalculator.MovementConsumption(playerController.MovementBehaviour, 
                playerController.LifeStatsManager);
            DrawGuiLabel($"Stamina Consumption: {consumption:F2}/s");
            DrawGuiLabel($"Can Run: {playerController.MovementBehaviour.CanRun()}");
            DrawGuiLabel($"Can Jump: {playerController.MovementBehaviour.CanJump()}");
            DrawGuiLabel($"Movement Efficiency: {playerController.MovementBehaviour.MovementEfficiency:F2}");

            
            _currentYPosition += 5f; // Bölümler arası küçük boşluk
            
            DrawGuiLabel($"Hunger : {playerController.LifeStatsManager.Hunger.Current:F}");
            DrawGuiLabel($"Thirst : {playerController.LifeStatsManager.Thirst.Current:F}");
            DrawGuiLabel($"Fatique : {playerController.LifeStatsManager.Fatique.Current:F}");
            
            _currentYPosition += 5f; // Bölümler arası küçük boşluk
            
            DrawGuiLabel($"Temperature isSafe : {playerController.LifeStatsManager.Temperature.IsSafe}");
            DrawGuiLabel($"Temperature : {playerController.LifeStatsManager.Temperature.Current}");
#endif
            
            
        }
    }
}