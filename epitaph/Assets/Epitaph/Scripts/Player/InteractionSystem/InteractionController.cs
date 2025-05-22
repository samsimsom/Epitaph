using System.Collections.Generic;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.InteractionSystem
{
    public class InteractionController : PlayerBehaviour
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        private Camera _playerCamera;

        private readonly List<PlayerBehaviour> _interactionBehaviours = new();
        
        public PlayerInteraction PlayerInteraction { get; private set; }
        
        public InteractionController(PlayerController playerController, 
            PlayerData playerData, Camera playerCamera) : base(playerController)
        {
            _playerController = playerController;
            _playerData = playerData;
            _playerCamera = playerCamera;
            
            InitializeBehaviours();
        }

        private T AddInteractionBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _interactionBehaviours.Add(behaviour);
            return behaviour;
        }

        public void InitializeBehaviours()
        {
            PlayerInteraction = AddInteractionBehaviour(new PlayerInteraction(_playerController, _playerData, _playerCamera));
        }

        public override void Awake()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.Awake();
        }

        public override void OnEnable()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnEnable();
        }

        public override void Start()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.Start();
        }

        public override void Update()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.Update();
        }

        public override void LateUpdate()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.LateUpdate();
        }

        public override void FixedUpdate()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.FixedUpdate();
        }

        public override void OnDisable()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnDisable();
        }

        public override void OnDestroy()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnDestroy();
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnDrawGizmos();
        }
#endif
    }
}