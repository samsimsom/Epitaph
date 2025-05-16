// ReSharper disable CommentTypo, IdentifierTypo, InvertIf
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera fpCamera;
        [SerializeField] private float xSensitivity = 20f;
        [SerializeField] private float ySensitivity = 20f;
        [SerializeField] private float referenceAspect = 16f/9f;
        
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
            var aspectMultiplier = referenceAspect / currentAspect;
            
            foreach (var controller in _inputAxisController.Controllers)
            {
                controller.Input.Gain = controller.Name switch
                {
                    "Look X (Pan)" => xSensitivity * aspectMultiplier,
                    "Look Y (Tilt)" => -ySensitivity,  // Dikey hassasiyeti sabit tutuyoruz
                    _ => controller.Input.Gain
                };
            }
        }
        
    }
}