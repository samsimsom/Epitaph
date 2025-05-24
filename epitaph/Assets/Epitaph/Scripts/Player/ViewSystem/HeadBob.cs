using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class HeadBob : PlayerBehaviour
    {
        private ViewBehaviour _viewBehaviour;
        private Vector3 _startPosition;
        private Vector3 _headBobOffset = Vector3.zero;

        public HeadBob(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(playerController)
        {
            _viewBehaviour = viewBehaviour;
        }

        public override void Start()
        {
            _startPosition = PlayerController.CameraTransform.localPosition;
        }

        public override void Update()
        {
            CheckForHeadBobTrigger();
            // Kamera pozisyonu diğer sınıflar tarafından değiştirilmiş olabileceğinden
            // başlangıç pozisyonunu sürekli güncelliyoruz, sadece y değerini koruyoruz
            _startPosition.x = PlayerController.CameraTransform.localPosition.x;
            _startPosition.z = PlayerController.CameraTransform.localPosition.z;
        }

        private void CheckForHeadBobTrigger()
        {
            if (PlayerController.MovementBehaviour.CurrentVelocity.sqrMagnitude >= 
                _viewBehaviour.HeadBobThreshold)
            {
                CalculateHeadBobAmount();
                CalculateHeadBobOffset();
            }
            else
            {
                ResetHeadBobOffset();
            }
            
            // Offseti uygula
            ApplyHeadBobOffset();
        }
        
        private void CalculateHeadBobOffset()
        {
            _headBobOffset = Vector3.zero;
            _headBobOffset.y += Mathf.Lerp(_headBobOffset.y, Mathf.Sin(Time.time * _viewBehaviour.HeadBobFrequency)
                * _viewBehaviour.HeadBobAmount * 1.4f, _viewBehaviour.HeadBobSmooth * Time.deltaTime);
            _headBobOffset.x += Mathf.Lerp(_headBobOffset.x, Mathf.Cos(Time.time * _viewBehaviour.HeadBobFrequency / 2f)
                * _viewBehaviour.HeadBobAmount * 1.6f, _viewBehaviour.HeadBobSmooth * Time.deltaTime);
        }

        private void CalculateHeadBobAmount()
        {
            if (PlayerController.MovementBehaviour.CurrentState.StateName == "WalkState")
            {
                _viewBehaviour.HeadBobAmount = 0.02f;
            }
            else if (PlayerController.MovementBehaviour.CurrentState.StateName == "RunState")
            {
                _viewBehaviour.HeadBobAmount = 0.03f;
            }
            else if (PlayerController.MovementBehaviour.CurrentState.StateName == "CrouchState")
            {
                _viewBehaviour.HeadBobAmount = 0.01f;
            }
            else
            {
                _viewBehaviour.HeadBobAmount = 0.02f;
            }
        }

        private void ResetHeadBobOffset()
        {
            // Debug.Log("Reset Head Bob Offset");
            _headBobOffset = Vector3.Lerp(_headBobOffset, Vector3.zero, 
                _viewBehaviour.HeadBobSmooth * Time.deltaTime);
            // _headBobOffset = Vector3.zero;
        }
        
        private void ApplyHeadBobOffset()
        {
            // Mevcut pozisyonu al ve sadece head bob offsetlerini uygula
            var currentPos = PlayerController.CameraTransform.localPosition;
            currentPos.x += _headBobOffset.x;
            currentPos.y += _headBobOffset.y;
            PlayerController.CameraTransform.localPosition = currentPos;
        }
    }
}