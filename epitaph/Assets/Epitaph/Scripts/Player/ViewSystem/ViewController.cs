using System.Collections.Generic;
using Epitaph.Scripts.Player.ScriptableObjects;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player.ViewSystem
{
    public class ViewController : PlayerBehaviour
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        
        private Camera _playerCamera;
        private CinemachineCamera _fpCamera; 
        private Transform _playerCameraTransform;

        private readonly List<PlayerBehaviour> _viewBehaviours = new();

        public PlayerLook PlayerLook { get; private set; }
        public PlayerHeadBob PlayerHeadBob { get; private set; }

        public ViewController(PlayerController playerController, PlayerData playerData,
            Camera playerCamera, CinemachineCamera fpCamera,
            Transform playerCameraTransform) : base(playerController)
        {
            _playerController = playerController;
            _playerData = playerData;
            _playerCamera = playerCamera;
            _fpCamera = fpCamera;
            _playerCameraTransform = playerCameraTransform;
            
            InitializeBehaviours();
        }

        private T AddViewBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _viewBehaviours.Add(behaviour);
            return behaviour;
        }

        public void InitializeBehaviours()
        {
            if (_playerCameraTransform != null)
            {
                PlayerLook = AddViewBehaviour(new PlayerLook(_playerController, _playerData, _playerCameraTransform, _fpCamera)); 
                PlayerHeadBob = AddViewBehaviour(new PlayerHeadBob(_playerController, _playerData, _playerCameraTransform));
            }
            else
            {
                Debug.LogError("PlayerCameraTransform is null in ViewController." +
                               "InitializeBehaviours. View behaviours cannot be " +
                               "initialized.", _playerController);
            }
        }

        public override void Awake()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Awake();
        }

        public override void OnEnable()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnEnable();
        }

        public override void Start()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Start();
        }

        public override void Update()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.Update();
        }

        public override void LateUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.LateUpdate();
        }

        public override void FixedUpdate()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.FixedUpdate();
        }

        public override void OnDisable()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDisable();
        }

        public override void OnDestroy()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDestroy();
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            foreach (var behaviour in _viewBehaviours) behaviour.OnDrawGizmos();
        }
#endif
    }
}