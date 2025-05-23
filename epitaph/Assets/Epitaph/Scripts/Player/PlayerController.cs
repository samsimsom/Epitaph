using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.InteractionSystem;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
using Epitaph.Scripts.Player.ViewSystem;
using Unity.Cinemachine;
using UnityEngine;

namespace Epitaph.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Data")]
        [SerializeField] private PlayerData playerData;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera playerCamera; 
        [SerializeField] private CinemachineCamera fpCamera; 
        [SerializeField] private Transform playerCameraTransform; 
        #endregion
        
        public PlayerData CurrentPlayerData { get; private set; }
        
        #region Player SubControllers
        private readonly List<PlayerBehaviour> _subControllers = new();
        public MovementController MovementController { get; private set; }
        public HealthController HealthController { get; private set; } 
        public InteractionController InteractionController { get; private set; } 
        public ViewController ViewController { get; private set; } 
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            if (playerData != null)
            {
                CurrentPlayerData = Instantiate(playerData);
                // CurrentPlayerData.InitializeDefaults();
            }
            else
            {
                Debug.LogError("Original PlayerData ScriptableObject atanmamış!");
            }
            
            InitializeBehaviours();
            
            foreach (var subController in _subControllers)
            {
                subController.Awake();
            }
        }
        
        private void OnEnable()
        {
            foreach (var subController in _subControllers)
            {
                subController.OnEnable();
            }
        }
        
        private void Start()
        {
            foreach (var subController in _subControllers)
            {
                subController.Start();
            }
        }
        
        private void Update()
        {
            foreach (var subController in _subControllers)
            {
                subController.Update();
            }
        }
        
        private void LateUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.LateUpdate();
            }
        }
        
        private void FixedUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.FixedUpdate();
            }
        }
        
        private void OnDisable()
        {
            foreach (var subController in _subControllers)
            {
                subController.OnDisable();
            }
        }
        
        private void OnDestroy()
        {
            foreach (var subController in _subControllers)
            {
                subController.OnDestroy();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // OnDrawGizmos çağrısı her subcontroller için null kontrolü yapmalı
            // veya _subControllers listesi null eleman içermemeli.
            if (_subControllers == null) return;
            foreach (var subController in _subControllers)
            {
                subController?.OnDrawGizmos();
            }
        }
#endif
        #endregion

        #region Initialization
        private T AddSubController<T>(T subController) where T : PlayerBehaviour
        {
            _subControllers.Add(subController);
            return subController;
        }
        
        private void InitializeBehaviours()
        {
            #region Inspector Objects
            // if (characterController == null) 
            //     characterController = GetComponent<CharacterController>();
            //
            // if (playerInput == null) 
            //     playerInput = GetComponent<PlayerInput>();
            //
            // if (playerCamera == null)
            // {
            //     var camComponent = GetComponentInChildren<Camera>();
            //     if (camComponent != null && camComponent.CompareTag("MainCamera"))
            //         playerCamera = camComponent;
            //     else
            //         Debug.LogError("PlayerCamera is not assigned in PlayerController " +
            //                        "and could not be found automatically.", this);
            // }
            //
            // if (fpCamera == null && playerCamera != null)
            // {
            //     if (playerCameraTransform != null)
            //         fpCamera = playerCameraTransform.GetComponentInChildren<CinemachineCamera>();
            // }
            //
            // if (playerCameraTransform == null)
            // {
            //     playerCameraTransform = transform.Find("FPCameraTransform");
            // }
            #endregion

            ViewController = AddSubController(new ViewController(this,
                CurrentPlayerData,
                playerCamera,
                fpCamera,
                playerCameraTransform));
            
            HealthController = AddSubController(new HealthController(this,
                CurrentPlayerData));

            MovementController = AddSubController(new MovementController(this,
                CurrentPlayerData,
                characterController,
                playerCameraTransform,
                HealthController,
                playerCamera));

            InteractionController = AddSubController(new InteractionController(this,
                CurrentPlayerData,
                playerCamera));
        }
        #endregion
        
        #region Public Accessor Methods
        public CharacterController GetCharacterController() => characterController;
        public PlayerData GetPlayerData() => playerData; 
        public PlayerInput GetPlayerInput() => playerInput;
        public Camera GetPlayerCamera() => playerCamera;
        public CinemachineCamera GetFpCamera() => fpCamera;
        public Transform GetPlayerCameraTransform() => playerCameraTransform;
        #endregion
    }
}