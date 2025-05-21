using System.Collections.Generic;
using Epitaph.Scripts.Player.ScriptableObjects;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class ViewController : IPlayerSubController
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        
        private Camera _playerCamera;
        private CinemachineCamera _fpCamera; 
        private Transform _playerCameraTransform;

        private readonly List<PlayerBehaviour> _viewBehaviours = new();

        public PlayerLook PlayerLook { get; private set; }
        public PlayerHeadBob PlayerHeadBob { get; private set; }

        public ViewController(Camera playerCamera, CinemachineCamera fpCamera, Transform playerCameraTransform)
        {
            _playerCamera = playerCamera;
            _fpCamera = fpCamera;
            _playerCameraTransform = playerCameraTransform;
        }

        private T AddViewBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _viewBehaviours.Add(behaviour);
            return behaviour;
        }

        public void InitializeBehaviours(PlayerController playerController, PlayerData playerData)
        {
            _playerController = playerController;
            _playerData = playerData;

            // PlayerLook ve PlayerHeadBob sınıflarının var olduğundan 
            // ve constructor imzalarının eşleştiğinden emin olun.
            if (_playerCameraTransform != null) // _playerCameraTransform null değilse devam et
            {
                // fpCamera null olabilir, PlayerLook constructor'ınız buna göre ayarlanmış olmalı
                PlayerLook = AddViewBehaviour(new PlayerLook(_playerController, _playerData, _playerCameraTransform, _fpCamera)); 
                PlayerHeadBob = AddViewBehaviour(new PlayerHeadBob(_playerController, _playerData, _playerCameraTransform));
            }
            else
            {
                Debug.LogError("PlayerCameraTransform is null in ViewController.InitializeBehaviours. View behaviours cannot be initialized.", _playerController);
            }
        }

        public void PlayerAwake()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Awake();
        }

        public void PlayerOnEnable()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnEnable();
        }

        public void PlayerStart()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Start();
        }

        public void PlayerUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Update();
        }

        public void PlayerLateUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.LateUpdate();
        }

        public void PlayerFixedUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.FixedUpdate();
        }

        public void PlayerOnDisable()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDisable();
        }

        public void PlayerOnDestroy()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDestroy();
        }

#if UNITY_EDITOR
        public void PlayerOnDrawGizmos()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDrawGizmos();
        }
#endif
    }
}