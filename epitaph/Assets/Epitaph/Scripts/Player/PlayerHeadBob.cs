using System;
using Epitaph.Scripts.Player.PlayerSO;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerHeadBob : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Transform playerCamera;

        public float amount = 0.05f;
        public float frequenct = 10.0f;
        public float smooth = 10.0f;
        public float treshold = 2.0f;
        
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = playerCamera.transform.localPosition;
        }

        private void Update()
        {
            if (playerData.currentVelocity.sqrMagnitude > treshold)
            {
                StartHeadBob();
            }
        }

        private Vector3 StartHeadBob()
        {
            var pos = Vector3.zero;
            pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * frequenct) 
                                       * amount * 1.4f, smooth * Time.deltaTime);
            pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * frequenct / 2f) 
                                       * amount * 1.6f, smooth * Time.deltaTime);
            playerCamera.transform.localPosition += pos;

            return pos;
        }

        private void StopHeadBob()
        {
            if (playerCamera.transform.localPosition == _startPosition) return;
            
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, _startPosition, smooth * Time.deltaTime);
        }
    }
}