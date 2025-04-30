using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera fpCamera;
        [SerializeField] private float xSensitivity;
        [SerializeField] private float ySensitivity;

        private float _xRotation;
        private CinemachineInputAxisController _inputAxisController;

        private void Awake()
        {
            _inputAxisController = fpCamera.GetComponent<CinemachineInputAxisController>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SetCinemachineInputGain();
        }

        private void SetCinemachineInputGain()
        {
            if (_inputAxisController == null) return;
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