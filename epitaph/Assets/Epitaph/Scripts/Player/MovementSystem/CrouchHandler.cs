using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class CrouchHandler : MovementSubBehaviour
    {
        // Crouch Configuration
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public float NormalCameraHeight = 1.5f;
        public float CrouchCameraHeight = 0.7f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);
        public float CrouchTransitionSpeed = 15f;

        // Target values for smooth transition
        private float _targetHeight;
        private Vector3 _targetCenter;

        public CrouchHandler(MovementBehaviour context, PlayerController playerController) 
            : base(context, playerController) { }

        public override void Start()
        {
            // Initialize to standing height
            _targetHeight = NormalHeight;
            _targetCenter = NormalControllerCenter;
            PlayerController.CharacterController.height = _targetHeight;
            PlayerController.CharacterController.center = _targetCenter;
            PlayerController.ViewBehaviour.SetCameraHeight(NormalCameraHeight);
        }
        
        public override void Update()
        {
            // Smoothly transition the CharacterController's height and center
            var controller = PlayerController.CharacterController;
            controller.height = Mathf.Lerp(controller.height, _targetHeight, Time.deltaTime * CrouchTransitionSpeed);
            controller.center = Vector3.Lerp(controller.center, _targetCenter, Time.deltaTime * CrouchTransitionSpeed);
        }

        public void HandleCrouch()
        {
            MovementBehaviour.IsCrouching = true;
            _targetHeight = CrouchHeight;
            _targetCenter = CrouchControllerCenter;
            PlayerController.ViewBehaviour.SetCameraHeight(CrouchCameraHeight);
        }

        public void HandleStandUp()
        {
            MovementBehaviour.IsCrouching = false;
            _targetHeight = NormalHeight;
            _targetCenter = NormalControllerCenter;
            PlayerController.ViewBehaviour.SetCameraHeight(NormalCameraHeight);
        }
        
        public bool CanStandUp()
        {
            // Cast a sphere upwards from the current position to check for obstacles
            var controller = PlayerController.CharacterController;
            var origin = PlayerController.transform.position + controller.center;
            var castDistance = (NormalHeight - controller.height); 
            var radius = controller.radius;
            var layerMask = ~LayerMask.GetMask("Player");

            // Return true if there are no obstacles above
            return !Physics.SphereCast(origin, radius, Vector3.up, out _, castDistance, layerMask);
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            VisualizeCrouchSpace();
        }

        private void VisualizeCrouchSpace()
        {
            if (!MovementBehaviour.IsCrouching || !Application.isPlaying) return;

            // Visualize the space needed to stand up
            var controller = PlayerController.CharacterController;
            var origin = PlayerController.transform.position + controller.center;
            var castDistance = (NormalHeight - controller.height);
            var radius = controller.radius;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin + Vector3.up * castDistance, radius);
        }
#endif
    }
}