using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerLook : PlayerBehaviour
    {
        private PlayerMovementData _playerMovementData;
        private PlayerController _playerController;
        private CinemachineCamera _fpCamera;
        
        public PlayerLook(PlayerController playerController, 
            PlayerMovementData playerMovementData,
            Camera playerCamera,
            CinemachineCamera fpCamera) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            _fpCamera = fpCamera;
        }

        public override void Start()
        {
            LockCursor();
            UpdateCameraSensitivity();
        }

        public override void OnEnable()
        {
            
        }

        public override void OnDisable()
        {
            
        }

        public override void Update()
        {
            
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UpdateCameraSensitivity()
        {
            // Mevcut en-boy oranı
            var currentAspect = (float)Screen.width / Screen.height;
            
            // En-boy oranı çarpanı
            var aspectMultiplier = _playerMovementData.referanceAspect / currentAspect;
            
            foreach (var controller in _fpCamera.GetComponent<CinemachineInputAxisController>().Controllers)
            {
                controller.Input.Gain = controller.Name switch
                {
                    "Look X (Pan)" => _playerMovementData.lookSensitivity.x * aspectMultiplier,
                    "Look Y (Tilt)" => -_playerMovementData.lookSensitivity.y * aspectMultiplier,
                    _ => controller.Input.Gain
                };
            }
        }
        
    }
}