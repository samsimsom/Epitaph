using System;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player;
using UnityEngine;

namespace Epitaph.Scripts.DebugPanel
{
    public class DebugLabel : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameTime gameTimeManager;

        private GUIStyle _labelStyle;

        private void Awake()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 20;
            _labelStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 300, 20), 
                $"Clock : {gameTimeManager.GameHour}:{gameTimeManager.GameMinute}", _labelStyle);
            
            GUI.Label(new Rect(10, 30, 300, 20), 
                $"Vertical Movement : {playerController.MovementBehaviour.VerticalMovement:F1}", _labelStyle);
            GUI.Label(new Rect(10, 50, 300, 20), 
                $"Capsul Velocity : {playerController.MovementBehaviour.CapsulVelocity}", _labelStyle);
            GUI.Label(new Rect(10, 70, 300, 20), 
                $"Current Movement : {playerController.MovementBehaviour.CurrentSpeed:F1}", _labelStyle);
            
            GUI.Label(new Rect(10, 90, 300, 20), 
                $"Is Grounded Custom: {playerController.MovementBehaviour.IsGrounded}", _labelStyle);
            GUI.Label(new Rect(10, 110, 300, 20), 
                $"Is Grounded Capsule: {playerController.CharacterController.isGrounded}", _labelStyle);
            
            GUI.Label(new Rect(10, 130, 300, 20), 
                $"Ground Normal : {playerController.MovementBehaviour.GroundNormal}", _labelStyle);
            
            GUI.Label(new Rect(10, 150, 300, 20), 
                $"Movement State : {playerController.MovementBehaviour.CurrentState.StateName}", _labelStyle);
        }
    }
}