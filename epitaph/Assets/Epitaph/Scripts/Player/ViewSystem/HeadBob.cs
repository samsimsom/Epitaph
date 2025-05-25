using System.Collections.Generic;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class HeadBob : PlayerBehaviour
    {
        private readonly ViewBehaviour _viewBehaviour;
        private Vector3 _headBobOffset = Vector3.zero;
        private Vector3 _lastHeadBobOffset = Vector3.zero;

        // Headbob amounts for different movement states
        private static readonly Dictionary<string, float> HeadBobAmounts = new()
        {
                { "WalkState", 0.01f },
                { "RunState", 0.02f },
                { "CrouchState", 0.005f },
                { "DefaultState", 0.01f }
            };

        public HeadBob(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(playerController)
        {
            _viewBehaviour = viewBehaviour;
        }

        public override void Start() { }

        public override void Update()
        {
            if (IsPlayerMovingAboveThreshold())
            {
                UpdateHeadBobAmount();
                CalculateHeadBobOffset();
            }
            else
            {
                ResetHeadBobOffset();
            }
            
            ApplyHeadBobIfChanged();
        }

        private bool IsPlayerMovingAboveThreshold()
        {
            return PlayerController.MovementBehaviour.CapsulVelocity.sqrMagnitude >= 
                   _viewBehaviour.HeadBobThreshold;
        }
        
        private void ApplyHeadBobIfChanged()
        {
            if (_headBobOffset != _lastHeadBobOffset)
            {
                _viewBehaviour.SetHeadBobOffset(_headBobOffset);
                _lastHeadBobOffset = _headBobOffset;
            }
        }
        
        private void CalculateHeadBobOffset()
        {
            _headBobOffset = Vector3.zero;
            
            var time = Time.time;
            var frequency = _viewBehaviour.HeadBobFrequency;
            var amount = _viewBehaviour.HeadBobAmount;
            
            // Calculate vertical and horizontal bob
            var verticalBob = Mathf.Sin(time * frequency) * amount * 1.4f;
            var horizontalBob = Mathf.Cos(time * frequency / 2f) * amount * 1.6f;
            
            _headBobOffset.y = verticalBob;
            _headBobOffset.x = horizontalBob;
        }

        private void UpdateHeadBobAmount()
        {
            var stateName = PlayerController.MovementBehaviour.CurrentState.StateName;
            
            if (HeadBobAmounts.TryGetValue(stateName, out var amount))
            {
                _viewBehaviour.HeadBobAmount = amount;
            }
            else
            {
                _viewBehaviour.HeadBobAmount = HeadBobAmounts["DefaultState"];
            }
        }

        private void ResetHeadBobOffset()
        {
            var resetSpeed = _viewBehaviour.HeadBobSmooth * 2f * Time.deltaTime;
            _headBobOffset = Vector3.Lerp(_headBobOffset, Vector3.zero, resetSpeed);
            
            // Zero out very small values to prevent tiny oscillations
            if (_headBobOffset.magnitude < 0.001f)
            {
                _headBobOffset = Vector3.zero;
            }
        }
        
    }
}