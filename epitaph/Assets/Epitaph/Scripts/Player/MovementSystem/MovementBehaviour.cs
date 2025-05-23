using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class MovementBehaviour : PlayerBehaviour
    {
        // State Variables
        private BaseState _currentState;
        private StateFactory _states;
        
        // Movement Variables
        public float WalkSpeed = 3.0f;
        public float RunSpeed = 6.0f;
        public float CrouchSpeed = 1.5f;

        // Jump Variables
        public float JumpForce = 8.0f;
        public float Gravity = 20.0f;
        private float _verticalVelocity;

        // Crouch Variables
        public float NormalHeight = 1.8f;
        public float CrouchHeight = 0.9f;
        public Vector3 NormalControllerCenter = new(0, 0.9f, 0);
        public Vector3 CrouchControllerCenter = new(0, 0.45f, 0);
        private bool _isCrouching;

        // Input Variables
        private Vector2 _currentMovementInput;
        private bool _isMovementPressed;
        private bool _isRunPressed;
        private bool _isJumpPressed;
        private bool _isCrouchPressedThisFrame; // Crouch için anlık basım

        // Getters & Setters
        public BaseState CurrentState 
        { 
            get => _currentState;
            set => _currentState = value;
        }
        public bool IsMovementPressed => _isMovementPressed;
        public bool IsRunPressed => _isRunPressed;
        public bool IsJumpPressed => _isJumpPressed;
        public bool IsCrouchPressedThisFrame => _isCrouchPressedThisFrame;
        public bool IsCrouching
        {
            get => _isCrouching;
            set => _isCrouching = value;
        }
        public float CurrentMovementY
        {
            get => _verticalVelocity;
            set => _verticalVelocity = value;
        }
        public float AppliedMovementX { get; set; }
        public float AppliedMovementZ { get; set; }

        // ---------------------------------------------------------------------------- //
        
        public MovementBehaviour(PlayerController playerController)
            : base(playerController) { }
        
        // ---------------------------------------------------------------------------- //
        
        public override void Awake()
        {
            _states = new StateFactory(this);
            _currentState = _states.Idle();
            _currentState.EnterState();
        }
    }
}