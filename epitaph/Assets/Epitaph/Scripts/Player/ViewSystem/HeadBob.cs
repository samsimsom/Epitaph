using System.Collections.Generic;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class HeadBob : ViewSubBehaviour
    {
        private readonly ViewBehaviour _viewBehaviour;
        private Vector3 _headBobOffset = Vector3.zero;
        private Vector3 _lastHeadBobOffset = Vector3.zero;

        // Headbob amounts for different movement states
        private static readonly Dictionary<string, float> HeadBobAmounts = new()
        {
            { "WalkState", 0.02f },
            { "RunState", 0.03f },
            { "CrouchState", 0.01f },
            { "DefaultState", 0.01f }
        };

        // Constants for HeadBob calculations
        private const float VerticalBobMultiplier = 1.4f;
        private const float HorizontalBobMultiplier = 1.6f;
        private const float BobInterpolationSpeed = 15f;
        private const float ResetSpeedMultiplier = 2f;
        private const float MinOffsetMagnitudeToZero = 0.001f;

        public HeadBob(ViewBehaviour viewBehaviour, PlayerController playerController)
            : base(viewBehaviour, playerController)
        {
            _viewBehaviour = viewBehaviour;
        }

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
            var time = Time.time;

            var frequency = _viewBehaviour.HeadBobFrequency;
            var amount = _viewBehaviour.HeadBobAmount;

            var targetVerticalBob = 0f;
            var targetHorizontalBob = 0f;

            // Calculate bobbing only if amount and frequency are meaningful
            if (amount > Mathf.Epsilon && frequency > Mathf.Epsilon)
            {
                // Calculate vertical and horizontal bobbing
                targetVerticalBob = Mathf.Sin(time * frequency) * amount * VerticalBobMultiplier;
                targetHorizontalBob = Mathf.Cos(time * frequency / 2f) * amount * HorizontalBobMultiplier;
            }
            // If amount or frequency is zero, targetVerticalBob and targetHorizontalBob will remain 0,
            // allowing the head bob to smoothly return to center.

            // Create the target head bob offset
            var currentTargetBobOffset = new Vector3(targetHorizontalBob, targetVerticalBob, 0f);

            // Smoothly interpolate the current _headBobOffset towards the calculated target
            _headBobOffset = Vector3.Lerp(_headBobOffset, currentTargetBobOffset,
                Time.deltaTime * BobInterpolationSpeed);
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
            var resetSpeed = _viewBehaviour.HeadBobSmooth * ResetSpeedMultiplier * 
                             Time.deltaTime;
            _headBobOffset = Vector3.Lerp(_headBobOffset, Vector3.zero, resetSpeed);
            
            // Zero out very small values to prevent tiny oscillations
            if (_headBobOffset.magnitude < MinOffsetMagnitudeToZero)
            {
                _headBobOffset = Vector3.zero;
            }
        }
    }
}