using System.Collections.Generic;
using Epitaph.Scripts.InputManager;
using Epitaph.Scripts.Player.BaseBehaviour;
using Epitaph.Scripts.Player.InteractionSystem;
using Epitaph.Scripts.Player.LifeStatsSystem;
using Epitaph.Scripts.Player.LifeStatsSystem.StatusEffects;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ViewSystem;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraTransform;
        
        // ---------------------------------------------------------------------------- //
        
        public PlayerInput PlayerInput => playerInput;
        public CharacterController CharacterController => characterController;
        public Camera PlayerCamera => playerCamera;
        public Transform CameraTransform => cameraTransform;
        
        // ---------------------------------------------------------------------------- //
        
        #region Player Behaviours
        
        private readonly List<PlayerBehaviour> _playerBehaviours = new();
        
        public MovementBehaviour MovementBehaviour { get; private set; }
        public ViewBehaviour ViewBehaviour { get; private set; }
        public InteractionBehaviour InteractionBehaviour { get; private set; }
        public LifeStatsManager LifeStatsManager { get; private set; }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
        #region Unity Lifecycle Methods

        private void Awake()
        {
            InitializeBehaviours();
            
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.Awake();
            }
        }

        private void OnEnable()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.OnEnable();
            }
        }

        private void Start()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.Start();
            }
        }

        private void Update()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.Update();
            }
        }

        private void LateUpdate()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.LateUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.FixedUpdate();
            }
        }

        private void OnDisable()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.OnDisable();
            }
        }

        private void OnDestroy()
        {
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.OnDestroy();
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (_playerBehaviours == null) return;
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour?.OnGUI();
            }
        }

        private void OnDrawGizmos()
        {
            if (_playerBehaviours == null) return;
            foreach (var behaviour in _playerBehaviours)
            {
                behaviour?.OnDrawGizmos();
            }
        }
#endif

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
        #region Initialization

        private T AddBehaviour<T>(T behaviour) where T : PlayerBehaviour
        {
            _playerBehaviours.Add(behaviour);
            return behaviour;
        }

        private void InitializeBehaviours()
        {
            ViewBehaviour = AddBehaviour(new ViewBehaviour(this));
            MovementBehaviour = AddBehaviour(new MovementBehaviour(this));
            InteractionBehaviour = AddBehaviour(new InteractionBehaviour(this));
            
            LifeStatsManager = AddBehaviour(new LifeStatsManager (this, 
                healthMax: 100f, vitalityMax: 100f, staminaMax: 100f, fatiqueMax: 100f, 
                thirstMax: 100f, hungerMax: 100f, temperatureMin: 28f, temperatureMax: 44f, 
                tempMinSafe: 36f, tempMaxSafe: 38f, tempStart: 37f)
            );
            
            LifeStatsManager.AddStatusEffect(new PoisonedEffect(10f));
            
            // Kaydet/yükle
            // var save = LifeStatsManager.SaveToJson();
            // var stats2 = new LifeStatsManager(this, 100,
            //     100,100,100,100,28,
            //     44,36,38,37);
            // stats2.LoadFromJson(save);
        }

        #endregion
        
        // ---------------------------------------------------------------------------- //
        
    }
}