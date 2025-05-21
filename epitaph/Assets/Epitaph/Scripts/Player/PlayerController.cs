using System.Collections.Generic;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.MovementSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
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
            // HealthController?.Awake(); // Bu satır kaldırılacak, döngü içinde hallediliyor.
        }
        
        private void OnEnable()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnEnable();
            }
            // HealthController?.OnEnable(); // Bu satır kaldırılacak
        }
        
        private void Start()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerStart();
            }
            // HealthController?.Start(); // Bu satır kaldırılacak
        }
        
        private void Update()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerUpdate();
            }
            // HealthController?.Update(); // Bu satır kaldırılacak
        }
        
        private void LateUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerLateUpdate();
            }
            // HealthController?.LateUpdate(); // Bu satır kaldırılacak
        }
        
        private void FixedUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerFixedUpdate();
            }
            // HealthController?.FixedUpdate(); // Bu satır kaldırılacak
        }
        
        private void OnDisable()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDisable();
            }
            // HealthController?.OnDisable(); // Bu satır kaldırılacak
        }
        
        private void OnDestroy()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDestroy();
            }
            // HealthController?.OnDestroy(); // Bu satır kaldırılacak
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
            // HealthController?.OnDrawGizmos(); // Bu satır kaldırılacak
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
            HealthController.InitializeBehaviours(this, playerData); // Bağımlılıkları enjekte et

            MovementController = AddSubController(new MovementController(characterController, playerCamera));
            // MovementController'ın HealthController'a erişimi varsa, bunu InitializeBehaviours içinde yapabilir
            // veya ayrı bir InjectDependencies metodu kalabilir. Şimdilik InjectDependencies kullanılıyor.
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
        public CinemachineCamera GetFPCamera() => fpCamera;
        public Transform GetPlayerCameraTransform() => playerCameraTransform;
        #endregion
        
        #region State Methods
        public bool IsSprinting() => playerData.isSprinting;
        public bool IsCrouching() => playerData.isCrouching;
        public bool IsGrounded() => playerData.isGrounded;
        #endregion
    }
}