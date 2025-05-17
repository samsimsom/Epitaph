using Epitaph.Scripts.Player.PlayerSO;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerMovementData playerMovementData;
        
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CinemachineCamera fpCamera;
        
        public Camera PlayerCamera => playerCamera;
        public Transform CameraTransform => fpCamera.transform;
        
        private CinemachineInputAxisController _inputAxisController;

        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _inputAxisController = fpCamera != null
                ? fpCamera.GetComponent<CinemachineInputAxisController>()
                : null;
        }

        private void Start()
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
            var aspectMultiplier = playerMovementData.referanceAspect / currentAspect;
            
            foreach (var controller in _inputAxisController.Controllers)
            {
                controller.Input.Gain = controller.Name switch
                {
                    "Look X (Pan)" => playerMovementData.lookSensitivity.x * aspectMultiplier,
                    "Look Y (Tilt)" => -playerMovementData.lookSensitivity.y * aspectMultiplier,
                    _ => controller.Input.Gain
                };
            }
        }
        
    }
}