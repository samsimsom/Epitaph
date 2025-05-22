using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Epitaph.Scripts.Player
{
    public class PlayerInput : MonoBehaviour, PlayerInputActions.IPlayerActions
    {
        public Vector2 mouseDelta;
        public Vector2 moveInput;
        public bool isMoveInput;
        
        private PlayerInputActions _playerInputActions;
        private PlayerController _playerController;

        // Eylem adlarına göre cooldown sürelerini saklar (saniye cinsinden)
        private Dictionary<string, float> _actionCooldownDurations = new Dictionary<string, float>();
        // Eylem adlarına göre bir sonraki kullanılabilir zaman damgalarını saklar
        private Dictionary<string, float> _nextActionTimestamps = new Dictionary<string, float>();
        
        private void Awake()
        {
            if (_playerInputActions != null) return;
            
            _playerController = GetComponent<PlayerController>();
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.SetCallbacks(this);

            // Cooldown uygulanacak eylemleri ve sürelerini burada tanımlayın
            // Eylem adları, Input Action Asset'inizdeki adlarla eşleşmelidir.
            RegisterActionCooldown("Crouch", 0.25f);
            RegisterActionCooldown("Interact", 0.5f);
            RegisterActionCooldown("Jump", 0.75f);
            // RegisterActionCooldown("Attack", 0.3f); 
        }

        private void RegisterActionCooldown(string actionName, float cooldown)
        {
            _actionCooldownDurations[actionName] = cooldown;
            _nextActionTimestamps[actionName] = 0f; // Başlangıçta hemen kullanılabilir
        }

        /// <summary>
        /// Belirtilen eylemin şu anda gerçekleştirilip gerçekleştirilemeyeceğini kontrol eder.
        /// </summary>
        /// <param name="actionName">Kontrol edilecek eylemin adı (Input Action Asset'teki adı).</param>
        /// <returns>Eylem gerçekleştirilebilirse true, aksi takdirde false.</returns>
        private bool CanPerformAction(string actionName)
        {
            if (_nextActionTimestamps.TryGetValue(actionName, out var nextTime))
            {
                return Time.time >= nextTime;
            }
            // Eğer eylem cooldown listesinde tanımlanmamışsa, cooldown yokmuş gibi davran
            return true;
        }

        /// <summary>
        /// Belirtilen eylem için cooldown'ı başlatır.
        /// </summary>
        /// <param name="actionName">Cooldown'ı başlatılacak eylemin adı.</param>
        private void StartActionCooldown(string actionName)
        {
            if (_actionCooldownDurations.TryGetValue(actionName, out var cooldownDuration))
            {
                _nextActionTimestamps[actionName] = Time.time + cooldownDuration;
            }
        }
        
        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Disable();
        }
        
        // ---------------------------------------------------------------------------- //
        #region Player Input Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            isMoveInput = moveInput.magnitude > 0.1f;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            mouseDelta = context.ReadValue<Vector2>();
            _playerController.ViewController?.PlayerLook?.SetMouseInput(mouseDelta);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            var actionName = context.action.name; // "Attack"
            if (context.performed && CanPerformAction(actionName))
            {
                // _playerController.AttackController?.PlayerAttack?.TryAttack();
                Debug.Log($"{actionName} gerçekleştirildi."); // Örnek
                StartActionCooldown(actionName);
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            var actionName = context.action.name; // "Interact"
            if (context.performed && CanPerformAction(actionName))
            {
                _playerController.InteractionController?.PlayerInteraction?.TryInteract();
                StartActionCooldown(actionName);
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            var actionName = context.action.name; // "Crouch"
            if (context.started && CanPerformAction(actionName))
            {
                _playerController.MovementController?.PlayerCrouch?.ToggleCrouch();
                StartActionCooldown(actionName);
            }
            else if (context.canceled)
            {
                // Gerekirse burası da güncellenir.
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            var actionName = context.action.name; // "Jump"
            if (context.performed && CanPerformAction(actionName))
            {
                _playerController.MovementController?.PlayerJump?.ProcessJump();
                StartActionCooldown(actionName);
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            // Bu eylem için cooldown gerekmiyorsa, olduğu gibi bırakılabilir.
            // Ya da gerekirse RegisterActionCooldown ile tanımlanıp CanPerformAction/StartActionCooldown kullanılabilir.
            if (context.performed)
            {
                GameTime.Instance.SkipTimeAsync(1f).Forget();
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            // Sprint gibi basılı tutma gerektiren eylemler farklı bir mantık gerektirebilir.
            // Bu cooldown sistemi daha çok tek seferlik basışlar (performed, started) için uygundur.
            if (context.performed)
            {
                _playerController.MovementController?.PlayerSprint?.TryStartSprint();
            }
            else if (context.canceled)
            {
                _playerController.MovementController?.PlayerSprint?.StopSprint();
            }
        }
        #endregion
        // ---------------------------------------------------------------------------- //
    }
}