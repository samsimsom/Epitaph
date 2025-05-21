using System.Collections.Generic;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

// Camera için

namespace Epitaph.Scripts.Player.MovementSystem
{
    public class InteractionController : IPlayerSubController
    {
        private PlayerController _playerController;
        private PlayerData _playerData;
        private Camera _playerCamera; // PlayerInteraction için gerekli olabilir

        private readonly List<PlayerBehaviour> _interactionBehaviours = new();

        // Etkileşim Davranışı
        public PlayerInteraction PlayerInteraction { get; private set; }

        // Constructor, PlayerInteraction'ın ihtiyaç duyduğu bağımlılıkları alabilir.
        // Örneğin, PlayerCamera.
        public InteractionController(Camera playerCamera)
        {
            _playerCamera = playerCamera;
        }

        private T AddInteractionBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _interactionBehaviours.Add(behaviour);
            return behaviour;
        }

        public void InitializeBehaviours(PlayerController playerController, PlayerData playerData)
        {
            _playerController = playerController;
            _playerData = playerData;

            // PlayerInteraction davranışını oluştur ve listeye ekle
            // Eski PlayerController'daki başlatmaya göre: new PlayerInteraction(this, playerData, playerCamera)
            // this -> _playerController olacak
            // playerCamera -> _playerCamera olacak (constructor'dan gelen)
            PlayerInteraction = AddInteractionBehaviour(new PlayerInteraction(_playerController, _playerData, _playerCamera));
        }

        public void PlayerAwake()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.Awake();
        }

        public void PlayerOnEnable()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnEnable();
        }

        public void PlayerStart()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.Start();
        }

        public void PlayerUpdate()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.Update();
        }

        public void PlayerLateUpdate()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.LateUpdate();
        }

        public void PlayerFixedUpdate()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.FixedUpdate();
        }

        public void PlayerOnDisable()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnDisable();
        }

        public void PlayerOnDestroy()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnDestroy();
        }

#if UNITY_EDITOR
        public void PlayerOnDrawGizmos()
        {
            foreach (var behaviour in _interactionBehaviours) behaviour.OnDrawGizmos();
        }
#endif
    }
}