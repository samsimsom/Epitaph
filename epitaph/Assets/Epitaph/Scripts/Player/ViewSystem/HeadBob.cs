using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class HeadBob : PlayerBehaviour
    {
        private ViewBehaviour _viewBehaviour;
        private Vector3 _headBobOffset = Vector3.zero;
        private Vector3 _lastHeadBobOffset = Vector3.zero; // Son offseti izlemek için

        public HeadBob(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(playerController)
        {
            _viewBehaviour = viewBehaviour;
        }

        public override void Start() { }

        public override void Update()
        {
            CheckForHeadBobTrigger();
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
            
            // Offset değişmişse sadece o zaman güncelle
            if (_headBobOffset != _lastHeadBobOffset)
            {
                _viewBehaviour.SetHeadBobOffset(_headBobOffset);
                _lastHeadBobOffset = _headBobOffset;
            }
        }
        
        private void CalculateHeadBobOffset()
        {
            // Değişimden önce _headBobOffset'i temizle
            _headBobOffset = Vector3.zero;
            
            // Yeni değerleri hesapla
            float bobY = Mathf.Sin(Time.time * _viewBehaviour.HeadBobFrequency) * 
                         _viewBehaviour.HeadBobAmount * 1.4f;
            float bobX = Mathf.Cos(Time.time * _viewBehaviour.HeadBobFrequency / 2f) * 
                         _viewBehaviour.HeadBobAmount * 1.6f;
            
            // Lerp yerine doğrudan değer atama - daha temiz hareket için
            _headBobOffset.y = bobY;
            _headBobOffset.x = bobX;
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
            // Daha hızlı sıfırlanma için çarpan
            var resetSpeed = _viewBehaviour.HeadBobSmooth * 2f * Time.deltaTime;
            
            // Lerp ile yumuşatma
            _headBobOffset = Vector3.Lerp(_headBobOffset, Vector3.zero, resetSpeed);
            
            // Çok küçük değerleri tamamen sıfırla
            if (_headBobOffset.magnitude < 0.001f)
            {
                _headBobOffset = Vector3.zero;
            }
        }
    }
}