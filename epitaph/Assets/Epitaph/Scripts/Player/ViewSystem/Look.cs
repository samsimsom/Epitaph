using Epitaph.Scripts.Player.BaseBehaviour;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class Look : PlayerBehaviour
    {
        private ViewBehaviour _viewBehaviour;
        private CinemachineCamera _cinemachineCamera;
        private CinemachineInputAxisController _inputAxis;

        public Vector2 LookSensivity => new(15f, 15f);

        public Look(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(playerController)
        {
            _viewBehaviour = viewBehaviour;
        }
        
        public override void Awake()
        {
            InitializeCinemachineComponents();
            SetCameraSensitivity();
        }

        public override void Start()
        {
            LockCursor();
        }
        
        private void InitializeCinemachineComponents()
        {
            var brain = PlayerController.PlayerCamera.GetComponent<CinemachineBrain>();
            var activeVirtual = brain.ActiveVirtualCamera;
            _cinemachineCamera = activeVirtual as CinemachineCamera;
            _inputAxis = _cinemachineCamera?.GetComponent<CinemachineInputAxisController>();
        }

        // ---------------------------------------------------------------------------- //
        
        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ---------------------------------------------------------------------------- //
        
        private void SetCameraSensitivity()
        {
            foreach (var controller in _inputAxis.Controllers)
            {
                controller.Input.Gain = controller.Name switch
                {
                    "Look X (Pan)" => LookSensivity.x,
                    "Look Y (Tilt)" => -LookSensivity.y,
                    _ => controller.Input.Gain
                };
            }
        }
        
        // ---------------------------------------------------------------------------- //
        
        
    }
}