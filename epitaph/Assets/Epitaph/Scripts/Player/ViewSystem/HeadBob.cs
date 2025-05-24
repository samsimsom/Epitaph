using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class HeadBob : PlayerBehaviour
    {
        private ViewBehaviour _viewBehaviour;
        private Vector3 _startPosition;

        public HeadBob(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(playerController)
        {
            _viewBehaviour = viewBehaviour;
        }
        
        public override void Start()
        {
            _startPosition = PlayerController.CameraTransform.transform.localPosition;
        }

        public override void Update()
        {
            CheckForHeadBobTrigger();
        }

        private void CheckForHeadBobTrigger()
        {
            if (PlayerController.MovementBehaviour.CurrentVelocity.sqrMagnitude >= 
                _viewBehaviour.HeadBobThreshold)
            {
                StartHeadBob();
            }
            else
            {
                StopHeadBob();
            }
        }

        private void StartHeadBob()
        {
            var pos = Vector3.zero;
            pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * _viewBehaviour.HeadBobFrequency) 
                                       * _viewBehaviour.HeadBobAmount * 1.4f, _viewBehaviour.HeadBobSmooth * Time.deltaTime);
            pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * _viewBehaviour.HeadBobFrequency / 2f) 
                                       * _viewBehaviour.HeadBobAmount * 1.6f, _viewBehaviour.HeadBobSmooth * Time.deltaTime);
            PlayerController.CameraTransform.localPosition += pos;
        }

        private void StopHeadBob()
        {
            if (PlayerController.CameraTransform.localPosition == _startPosition) return;

            if (PlayerController.MovementBehaviour.IsCrouching)
            {
                PlayerController.CameraTransform.localPosition = Vector3.Lerp(
                    PlayerController.CameraTransform.localPosition, 
                    new Vector3(_startPosition.x, 
                        PlayerController.MovementBehaviour.CrouchCameraHeight, 
                        _startPosition.z),
                    _viewBehaviour.HeadBobSmooth * Time.deltaTime);
            }
            
        }
    }
}