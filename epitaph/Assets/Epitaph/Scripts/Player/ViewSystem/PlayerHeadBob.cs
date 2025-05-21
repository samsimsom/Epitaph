using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class PlayerHeadBob : PlayerBehaviour
    {
        private PlayerData _playerData;
        private Transform _playerCameraTransform;
        private Vector3 _startPosition;

        public PlayerHeadBob(PlayerController playerController, 
            PlayerData playerData, 
            Transform playerCameraTransform) : base(playerController)
        {
            _playerData = playerData;
            _playerCameraTransform = playerCameraTransform;
        }

        public override void Start()
        {
            _startPosition = _playerCameraTransform.transform.localPosition;
        }

        public override void Update()
        {
            CheckForHeadBobTrigger();
            StopHeadBob();
        }

        private void CheckForHeadBobTrigger()
        {
            if (_playerData.currentVelocity.sqrMagnitude > _playerData.treshold)
            {
                StartHeadBob();
            }
        }

        private void StartHeadBob()
        {
            var pos = Vector3.zero;
            pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * _playerData.frequenct) 
                                       * _playerData.amount * 1.4f, _playerData.smooth * Time.deltaTime);
            pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * _playerData.frequenct / 2f) 
                                       * _playerData.amount * 1.6f, _playerData.smooth * Time.deltaTime);
            _playerCameraTransform.localPosition += pos;
        }

        private void StopHeadBob()
        {
            if (_playerCameraTransform.localPosition == _startPosition) return;
            
            _playerCameraTransform.localPosition = Vector3.Lerp(
                _playerCameraTransform.localPosition, _startPosition,
                _playerData.smooth * Time.deltaTime);
        }
    }
}