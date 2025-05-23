using Epitaph.Scripts.Player.Input;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraTransform;

    }
}