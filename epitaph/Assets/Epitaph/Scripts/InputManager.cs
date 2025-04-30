using Epitaph.Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Epitaph.Scripts
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerLook playerLook;
        
        private InputSystem_Actions _inputSystemActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        private void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _playerActions = _inputSystemActions.Player;

            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }
            
            if (playerLook == null)
            {
                playerLook = GetComponent<PlayerLook>();
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
            playerMovement.ProcessMove(_playerActions.Move.ReadValue<Vector2>());
        }
    }
}
