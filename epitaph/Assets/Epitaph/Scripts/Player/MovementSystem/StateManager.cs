using Epitaph.Scripts.Player.MovementSystem.StateMachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class StateManager : MovementSubBehaviour
    {
        private StateFactory _states;
        
        public StateBase CurrentState { get; set; }

        // Factory metotlarına erişim
        public StateBase GetIdleState() => _states.Idle();
        public StateBase GetWalkState() => _states.Walk();
        public StateBase GetRunState() => _states.Run();
        public StateBase GetJumpState() => _states.Jump();
        public StateBase GetFallState() => _states.Fall();
        public StateBase GetCrouchState() => _states.Crouch();
        
        public StateManager(MovementBehaviour movementBehaviour, PlayerController playerController)
            : base(movementBehaviour, playerController) { }

        public override void Awake()
        {
            _states = new StateFactory(MovementBehaviour);
            CurrentState = _states.Idle();
            CurrentState.EnterState();
        }

        public override void Update()
        {
            CurrentState?.UpdateState();
        }

        public override void FixedUpdate()
        {
            CurrentState?.FixedUpdateState();
        }

        public void SwitchState(StateBase newState)
        {
            CurrentState?.ExitState();
            CurrentState = newState;
            CurrentState?.EnterState();
        }

#if UNITY_EDITOR
        public override void OnGUI()
        {
            GUI.Label(new Rect(10, 90, 300, 20), $"Current State: {CurrentState?.StateName ?? "None"}");
            
            if (CurrentState != null)
            {
                GUI.Label(new Rect(10, 110, 300, 20), $"State Type: {CurrentState.GetType().Name}");
            }
        }

        public override void OnDrawGizmos()
        {
            CurrentState?.OnDrawGizmos();
        }
#endif
    }
}