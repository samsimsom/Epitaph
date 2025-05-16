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
            foreach (var controller in _inputAxisController.Controllers)
            {
                controller.Input.Gain = controller.Name switch
                {
                    "Look X (Pan)" => xSensitivity,
                    "Look Y (Tilt)" => -ySensitivity,
                    _ => controller.Input.Gain
                };
            }
        }
        
    }
}