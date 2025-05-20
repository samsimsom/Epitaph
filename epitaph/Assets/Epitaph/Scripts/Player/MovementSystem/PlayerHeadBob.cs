using Epitaph.Scripts.Player.ScriptableObjects.MovementSO;
using UnityEngine;

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class PlayerHeadBob : PlayerBehaviour
    {
        private PlayerMovementData _playerMovementData;
        private Transform _playerCameraTransform;
        
        private Vector3 _startPosition;

        public PlayerHeadBob(PlayerController playerController, 
            PlayerMovementData playerMovementData, 
            Transform playerCameraTransform) : base(playerController)
        {
            _playerMovementData = playerMovementData;
            _playerCameraTransform = playerCameraTransform;
        }

        public override void Start()
        {
            _startPosition = _playerCameraTransform.transform.localPosition;
        }

        public override void OnEnable()
        {
            
        }

        public override void OnDisable()
        {
            
        }

        public override void Update()
        {
            CheckForHeadBobTrigger();
            StopHeadBob();
        }

        private void CheckForHeadBobTrigger()
        {
            if (_playerMovementData.currentVelocity.sqrMagnitude > _playerMovementData.treshold)
            {
                StartHeadBob();
            }
        }

        private void StartHeadBob()
        {
            var pos = Vector3.zero;
            pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * _playerMovementData.frequenct) 
                                       * _playerMovementData.amount * 1.4f, _playerMovementData.smooth * Time.deltaTime);
            pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * _playerMovementData.frequenct / 2f) 
                                       * _playerMovementData.amount * 1.6f, _playerMovementData.smooth * Time.deltaTime);
            _playerCameraTransform.localPosition += pos;

            // return pos;
        }

        private void StopHeadBob()
        {
            if (_playerCameraTransform.localPosition == _startPosition) return;
            
            _playerCameraTransform.localPosition = Vector3.Lerp(
                _playerCameraTransform.localPosition, _startPosition,
                _playerMovementData.smooth * Time.deltaTime);
        }
    }
}