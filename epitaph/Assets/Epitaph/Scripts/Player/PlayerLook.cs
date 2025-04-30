using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera fpCamera;
        [SerializeField] private float xSensitivity = 1f;
        [SerializeField] private float ySensitivity = 1f;

        private CinemachineInputAxisController _inputAxisController;

        private void Awake()
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

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UpdateCameraSensitivity()
        {
            if (_inputAxisController == null || _inputAxisController.Controllers == null) return;

            foreach (var controller in _inputAxisController.Controllers)
            {
                switch (controller.Name)
                {
                    case "Look X (Pan)":
                        controller.Input.Gain = xSensitivity;
                        break;
                    case "Look Y (Tilt)":
                        controller.Input.Gain = -ySensitivity;
                        break;
                }
            }
        }

        // Sensitivity atama istekleri için dışarıya açık fonksiyonlar
        public void SetXSensitivity(float value)
        {
            xSensitivity = value;
            UpdateCameraSensitivity();
        }

        public void SetYSensitivity(float value)
        {
            ySensitivity = value;
            UpdateCameraSensitivity();
        }
    }
}