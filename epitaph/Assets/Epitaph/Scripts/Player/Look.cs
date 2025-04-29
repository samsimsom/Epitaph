using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class Look : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera fpCamera;
        [SerializeField] private Transform fpCameraTransform;
        [SerializeField] private float xSensitivity;
        [SerializeField] private float ySensitivity;

        private float _xRotation;
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            var inputAxisController = fpCamera.GetComponent<CinemachineInputAxisController>();
            if (inputAxisController != null)
            {
                foreach (var controller in inputAxisController.Controllers)
                {
                    if (controller.Name == "Look X (Pan)")
                    {
                        controller.Input.Gain = xSensitivity;
                    }
                    
                    if (controller.Name == "Look Y (Tilt)")
                    {
                        controller.Input.Gain = -ySensitivity;
                    }
                }
            }

            
        }

        public void ProcessLook(Vector2 input)
        {
            var mouseX = input.x;
            var mouseY = input.y;
            
            transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * ySensitivity));
            _xRotation -= mouseY * Time.deltaTime * xSensitivity;
            _xRotation = Mathf.Clamp(_xRotation, -85f, 85f);
            fpCameraTransform.localEulerAngles = new Vector3(_xRotation, fpCameraTransform.localEulerAngles.y, 0f);
            fpCameraTransform.localRotation = Quaternion.Euler(0f, _xRotation, 0f);
        }
    }
}