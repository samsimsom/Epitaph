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
        
        public MovementBehaviour(PlayerController controller) : base(controller) { }
        
        public override void Awake()
        {
            _states = new StateFactory(this);
            _currentState = _states.Idle();
            _currentState.EnterState();
        }
    }
}