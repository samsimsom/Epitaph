using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Epitaph.Scripts.Player
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float speed;

        private void Start()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }

        public void ProcessMove(Vector2 input)
        {
            var moveDirection = Vector3.zero;
            moveDirection.x = input.x;
            moveDirection.z = input.y;
            moveDirection = transform.TransformDirection(moveDirection);
            characterController.Move(moveDirection * (speed * Time.deltaTime));
        }
    }
}