using System;
using Epitaph.Scripts.Player;
using UnityEngine;

namespace Epitaph.Scripts
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Movement movement;
        [SerializeField] private Look look;
        
        private InputSystem_Actions _inputSystemActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        private void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _playerActions = _inputSystemActions.Player;

            if (movement == null)
            {
                movement = GetComponent<Movement>();
            }
            
            if (look == null)
            {
                look = GetComponent<Look>();
            }
        }
        
        private void OnEnable()
        {
            _playerActions.Enable();
        }

        private void OnDisable()
        {
            _playerActions.Disable();
        }

        private void Update()
        {
            movement.ProcessMove(_playerActions.Move.ReadValue<Vector2>());
        }

        private void LateUpdate()
        {
            look.ProcessLook(_playerActions.Look.ReadValue<Vector2>());
        }
    }
}
