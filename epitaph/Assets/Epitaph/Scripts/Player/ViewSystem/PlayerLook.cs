using Epitaph.Scripts.Player.ScriptableObjects;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class PlayerLook : PlayerBehaviour
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        private Transform _playerCameraTransform;
        private CinemachineCamera _fpCamera;

        public PlayerLook(PlayerController playerController,
            PlayerData playerData,
            Transform playerCameraTransform,
            CinemachineCamera fpCamera) : base(playerController)
        {
            _playerData = playerData;
            _playerCameraTransform = playerCameraTransform;
            _fpCamera = fpCamera;
        }

        public override void Start()
        {
            LockCursor();
            UpdateCameraSensitivity();
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

        public void SetMouseInput(Vector2 input)
        {
            
        }
    }
}