using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraTransform;

        public CharacterController CharacterController => characterController;
        public Camera PlayerCamera => playerCamera;
        public CinemachineCamera FpCamera
        {
            get
            {
                var cinemachineBrain = playerCamera.GetComponent<CinemachineBrain>();
                return cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
            }
        }
        public Transform CameraTransform => cameraTransform;
        
        
        // ---------------------------------------------------------------------------- //

        private void Start()
        {
            Debug.Log(FpCamera.name);
        }
        
        // ---------------------------------------------------------------------------- //
        
    }
}
