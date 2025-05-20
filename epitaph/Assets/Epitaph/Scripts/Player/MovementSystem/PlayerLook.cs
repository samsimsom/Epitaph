using Epitaph.Scripts.Player.ScriptableObjects;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerLook : PlayerBehaviour
    {
        private PlayerData _playerData;
        private PlayerController _playerController;
        private CinemachineCamera _fpCamera;
        
        public PlayerLook(PlayerController playerController, 
            PlayerData playerData,
            Camera playerCamera,
            CinemachineCamera fpCamera) : base(playerController)
        {
            _playerData = playerData;
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

        public override void OnDrawGizmos()
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
            var aspectMultiplier = _playerData.referanceAspect / currentAspect;
            
            foreach (var controller in _fpCamera.GetComponent<CinemachineInputAxisController>().Controllers)
            {
                controller.Input.Gain = controller.Name switch
                {
                    "Look X (Pan)" => _playerData.lookSensitivity.x * aspectMultiplier,
                    "Look Y (Tilt)" => -_playerData.lookSensitivity.y * aspectMultiplier,
                    _ => controller.Input.Gain
                };
            }
        }
        
    }
}