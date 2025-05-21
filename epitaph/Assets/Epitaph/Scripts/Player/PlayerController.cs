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
        
        #region Player SubControllers
        private readonly List<IPlayerSubController> _subControllers = new();
        
        public MovementController MovementController { get; private set; }
        public HealthController HealthController { get; private set; } 
        public InteractionController InteractionController { get; private set; } 
        public ViewController ViewController { get; private set; } 
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeSubControllersAndBehaviours();
            foreach (var subController in _subControllers)
            {
                subController.PlayerAwake();
            }
        }
        
        private void OnEnable()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnEnable();
            }
        }
        
        private void Start()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerStart();
            }
        }
        
        private void Update()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerUpdate();
            }
        }
        
        private void LateUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerLateUpdate();
            }
        }
        
        private void FixedUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerFixedUpdate();
            }
        }
        
        private void OnDisable()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDisable();
            }
        }
        
        private void OnDestroy()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDestroy();
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
                subController?.PlayerOnDrawGizmos();
            }
        }
#endif
        #endregion

        #region Initialization
        private T AddSubController<T>(T subController) where T : IPlayerSubController
        {
            _subControllers.Add(subController);
            return subController;
        }
        
        private void InitializeSubControllersAndBehaviours()
        {
            if (characterController == null) 
                characterController = GetComponent<CharacterController>();

            if (playerInput == null) 
                playerInput = GetComponent<PlayerInput>();
            
            if (playerCamera == null)
            {
                var camComponent = GetComponentInChildren<Camera>();
                if (camComponent != null && camComponent.CompareTag("MainCamera"))
                    playerCamera = camComponent;
                else
                    Debug.LogError("PlayerCamera is not assigned in PlayerController and could not be found automatically.", this);
            }

            if (fpCamera == null && playerCamera != null)
            {
                if (playerCameraTransform != null)
                     fpCamera = playerCameraTransform.GetComponentInChildren<CinemachineCamera>();
            }

            if (playerCameraTransform == null && playerCamera != null)
            {
                playerCameraTransform = playerCamera.transform;
            }
            
            // HealthController'ı oluştur ve listeye ekle
            HealthController = AddSubController(new HealthController()); 
            HealthController.InitializeBehaviours(this, playerData);

            MovementController = AddSubController(new MovementController(characterController, playerCamera));
            MovementController.InjectDependencies(HealthController); 
            MovementController.InitializeBehaviours(this, playerData);
            
            InteractionController = AddSubController(new InteractionController(playerCamera)); 
            InteractionController.InitializeBehaviours(this, playerData);
            
            if (playerCamera != null && playerCameraTransform != null)
            {
                ViewController = AddSubController(new ViewController(playerCamera, fpCamera, playerCameraTransform));
                ViewController.InitializeBehaviours(this, playerData);
            }
            else
            {
                Debug.LogError("Cannot initialize ViewController because playerCamera or playerCameraTransform is missing!", this);
            }
        }
        #endregion
        
        #region Public Accessor Methods
        public CharacterController GetCharacterController() => characterController;
        public PlayerData GetPlayerData() => playerData; 
        public PlayerInput GetPlayerInput() => playerInput;
        // HealthController'a erişim zaten public bir property olarak tanımlı
        // public HealthController GetHealthController() => HealthController; 

        public Camera GetPlayerCamera() => playerCamera;
        public CinemachineCamera GetFpCamera() => fpCamera;
        public Transform GetPlayerCameraTransform() => playerCameraTransform;
        #endregion
        
        #region State Methods
        public bool IsSprinting() => playerData.isSprinting;
        public bool IsCrouching() => playerData.isCrouching;
        public bool IsGrounded() => playerData.isGrounded;
        #endregion
    }
}