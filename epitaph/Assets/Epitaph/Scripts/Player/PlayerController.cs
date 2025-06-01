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
        
        // Activity levels for different movement states
        private const float IDLE_ACTIVITY = 0f;
        private const float WALK_ACTIVITY = 0.5f;
        private const float RUN_ACTIVITY = 1.5f;
        private const float JUMP_ACTIVITY = 2.0f;
        private const float CROUCH_ACTIVITY = 0.3f;
        
        // Stamina consumption rates
        private const float STAMINA_WALK_RATE = 5f;
        private const float STAMINA_RUN_RATE = 15f;
        private const float STAMINA_JUMP_COST = 10f;
        
        // Speed modifiers based on life stats
        private float _speedModifier = 1f;
        private float _baseWalkSpeed;
        private float _baseRunSpeed;
        
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
            // Store original speeds
            _baseWalkSpeed = MovementBehaviour.WalkSpeed;
            _baseRunSpeed = MovementBehaviour.RunSpeed;

            foreach (var behaviour in _playerBehaviours)
            {
                behaviour.Start();
            }
        }

        private void Update()
        {
            UpdateSpeedModifiers();
            HandleStaminaConsumption();
            ApplyMovementRestrictions();

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
        
        private void UpdateSpeedModifiers()
        {
            // Calculate speed modifier based on life stats
            var fatigueModifier = CalculateFatigueModifier();
            var thirstModifier = CalculateThirstModifier();
            var hungerModifier = CalculateHungerModifier();
            var vitalityModifier = CalculateVitalityModifier();
            
            // Combine all modifiers (multiplicative)
            _speedModifier = fatigueModifier * thirstModifier * hungerModifier * vitalityModifier;
            
            // Apply speed modifications
            MovementBehaviour.WalkSpeed = _baseWalkSpeed * _speedModifier;
            MovementBehaviour.RunSpeed = _baseRunSpeed * _speedModifier;
        }
        
        private float CalculateFatigueModifier()
        {
            var fatigueRatio = LifeStatsManager.Fatique.Current / LifeStatsManager.Fatique.Max;
            
            if (fatigueRatio < 0.3f) return 1f; // No penalty when fatigue is low
            if (fatigueRatio < 0.6f) return 0.9f; // 10% speed reduction
            if (fatigueRatio < 0.8f) return 0.75f; // 25% speed reduction
            return 0.5f; // 50% speed reduction when critically fatigued
        }

        private float CalculateThirstModifier()
        {
            var thirstRatio = LifeStatsManager.Thirst.Current / LifeStatsManager.Thirst.Max;
            
            if (thirstRatio < 0.4f) return 1f;
            if (thirstRatio < 0.7f) return 0.95f;
            if (thirstRatio < 0.9f) return 0.85f;
            return 0.7f; // Severely dehydrated
        }
        
        private float CalculateHungerModifier()
        {
            var hungerRatio = LifeStatsManager.Hunger.Current / LifeStatsManager.Hunger.Max;
            
            if (hungerRatio < 0.4f) return 1f;
            if (hungerRatio < 0.7f) return 0.95f;
            if (hungerRatio < 0.9f) return 0.9f;
            return 0.8f; // Very hungry
        }

        private float CalculateVitalityModifier()
        {
            var vitalityRatio = LifeStatsManager.Vitality.Current / LifeStatsManager.Vitality.Max;
            return Mathf.Lerp(0.6f, 1f, vitalityRatio); // Linear scaling from 60% to 100%
        }

        private void HandleStaminaConsumption()
        {
            var deltaTime = Time.deltaTime;
            
            if (MovementBehaviour.IsRunning)
            {
                // Running consumes more stamina
                var consumptionRate = STAMINA_RUN_RATE;
                
                // Increase consumption if fatigued
                if (LifeStatsManager.Fatique.IsCritical)
                    consumptionRate *= 1.5f;
                
                LifeStatsManager.DecreaseStamina(deltaTime, consumptionRate);
                LifeStatsManager.DecreaseFatique(deltaTime, RUN_ACTIVITY);
                LifeStatsManager.DecreaseThirst(deltaTime, RUN_ACTIVITY);
                LifeStatsManager.DecreaseHunger(deltaTime, RUN_ACTIVITY);
            }
            else if (MovementBehaviour.IsWalking)
            {
                // Walking consumes less stamina
                LifeStatsManager.DecreaseStamina(deltaTime, STAMINA_WALK_RATE);
                LifeStatsManager.DecreaseFatique(deltaTime, WALK_ACTIVITY);
                LifeStatsManager.DecreaseThirst(deltaTime, WALK_ACTIVITY);
                LifeStatsManager.DecreaseHunger(deltaTime, WALK_ACTIVITY);
            }
            else if (MovementBehaviour.IsCrouching)
            {
                // Crouching activity
                LifeStatsManager.DecreaseFatique(deltaTime, CROUCH_ACTIVITY);
                LifeStatsManager.DecreaseThirst(deltaTime, CROUCH_ACTIVITY);
                LifeStatsManager.DecreaseHunger(deltaTime, CROUCH_ACTIVITY);
            }
            else
            {
                // Resting - regenerate stamina
                LifeStatsManager.IncreaseStamina(deltaTime);
            }
            
            // Jumping costs stamina
            if (MovementBehaviour.IsJumping)
            {
                LifeStatsManager.AddStat("Stamina", -STAMINA_JUMP_COST);
            } 
        }


        private void ApplyMovementRestrictions()
        {
            // Prevent running if stamina is too low
            if (LifeStatsManager.Stamina.Current < 10f)
            {
                // Force walk mode or slower movement
                // Bu kısmı MovementBehaviour'da implement etmeniz gerekecek
                RestrictHighEnergyMovement();
            }

            // Prevent jumping if stamina is insufficient
            if (LifeStatsManager.Stamina.Current < STAMINA_JUMP_COST)
            {
                RestrictJumping();
            }
        }

        private void RestrictHighEnergyMovement()
        {
            // MovementBehaviour'a stamina restriction flag'i ekleyebilirsiniz
            Debug.Log("Low stamina - restricting high energy movement");
        }

        private void RestrictJumping()
        {
            // Jump restriction logic
            Debug.Log("Insufficient stamina for jumping");
        }

        
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