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
        [SerializeField] private Camera playerCamera; // Ana oyuncu kamerası
        [SerializeField] private CinemachineCamera fpCamera; // Cinemachine kamerası
        [SerializeField] private Transform playerCameraTransform; // Genellikle kameranın parent'ı olan ve döndürülen transform
        #endregion
        
        #region Player SubControllers
        private readonly List<IPlayerSubController> _subControllers = new();
        
        // Alt Kontrolcüler
        public MovementController MovementController { get; private set; }
        public HealthController HealthController { get; private set; } 
        public InteractionController InteractionController { get; private set; } 
        public ViewController ViewController { get; private set; } // Yeni eklendi
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeSubControllersAndBehaviours();
            foreach (var subController in _subControllers)
            {
                subController.PlayerAwake();
            }
            // HealthController ayrı yönetiliyorsa onun Awake'i çağrılabilir.
            HealthController?.Awake(); // Eğer HealthController PlayerBehaviour ise. Şimdiki yapıda değil.
        }
        
        private void OnEnable()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnEnable();
            }
            // HealthController?.OnEnable();
        }
        
        private void Start()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerStart();
            }
            // HealthController?.Start();
        }
        
        private void Update()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerUpdate();
            }
            // HealthController?.Update();
        }
        
        private void LateUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerLateUpdate();
            }
            // HealthController?.LateUpdate();
        }
        
        private void FixedUpdate()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerFixedUpdate();
            }
            // HealthController?.FixedUpdate();
        }
        
        private void OnDisable()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDisable();
            }
            // HealthController?.OnDisable();
        }
        
        private void OnDestroy()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDestroy();
            }
            // HealthController?.OnDestroy();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (var subController in _subControllers)
            {
                subController.PlayerOnDrawGizmos();
            }
            // HealthController?.OnDrawGizmos();
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
            // Get required components if not already assigned
            if (characterController == null) 
                characterController = GetComponent<CharacterController>();

            if (playerInput == null) 
                playerInput = GetComponent<PlayerInput>();
            
            // Componentlerin atanıp atanmadığını kontrol et (Inspector'dan atanmamış olabilirler)
            if (playerCamera == null)
            {
                // Genellikle ana kamera Tag ile bulunur veya direkt atanır.
                // Bu örnekte, child bir obje olduğunu varsayarak bulmaya çalışabiliriz
                // ya da Inspector'dan atanması zorunlu kılınabilir.
                var camComponent = GetComponentInChildren<Camera>();
                if (camComponent != null && camComponent.CompareTag("MainCamera")) // Veya özel bir tag/isim
                    playerCamera = camComponent;
                else
                    Debug.LogError("PlayerCamera is not assigned in PlayerController and could not be found automatically.", this);
            }

            if (fpCamera == null && playerCamera != null)
            {
                // Eğer playerCamera bir Cinemachine Brain ise veya fpCamera direkt atanmamışsa,
                // CinemachineCamera'yı playerCamera objesinde veya bir child objede arayabiliriz.
                // Bu kısım projenizin yapısına göre değişir.
                // Genellikle fpCamera direkt olarak Inspector'dan atanır.
                // Eğer PlayerCameraTransform üzerinde bir CinemachineVirtualCamera varsa:
                if (playerCameraTransform != null)
                     fpCamera = playerCameraTransform.GetComponentInChildren<CinemachineCamera>(); // Veya GetComponent<CinemachineBrain>() ve oradan aktif kamera.
                // else
                // Debug.LogWarning("fpCamera or playerCameraTransform is not assigned. ViewController might not function correctly.", this);
            }

            if (playerCameraTransform == null && playerCamera != null)
            {
                // Genellikle playerCamera'nın parent'ı ya da kendisi olur.
                // PlayerLook genellikle bu transformu döndürür.
                playerCameraTransform = playerCamera.transform; // Veya kameranın parent'ıysa playerCamera.transform.parent;
                // Debug.LogWarning("playerCameraTransform is not assigned. Assigning playerCamera.transform by default.", this);
            }
            
            HealthController = new HealthController(this, playerData); 

            MovementController = AddSubController(new MovementController(characterController, playerCamera));
            MovementController.InjectDependencies(HealthController); 
            MovementController.InitializeBehaviours(this, playerData);
            
            InteractionController = AddSubController(new InteractionController(playerCamera)); 
            InteractionController.InitializeBehaviours(this, playerData);
            
            // Initialize ViewController
            // Gerekli componentlerin null olup olmadığını kontrol et veya varsayılan ata
            if (playerCamera != null && playerCameraTransform != null) // fpCamera opsiyonel olabilir, PlayerLook'a bağlı
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
        public HealthController GetHealthController() => HealthController; 

        // Kamera ile ilgili bileşenlere erişim için getter'lar (ViewController veya PlayerInput kullanabilir)
        public Camera GetPlayerCamera() => playerCamera;
        public CinemachineCamera GetFPCamera() => fpCamera;
        public Transform GetPlayerCameraTransform() => playerCameraTransform;
        #endregion
        
        #region State Methods
        // Bu metotlar hala geçerli olabilir veya ilgili alt kontrolcülere taşınabilir.
        public bool IsSprinting() => playerData.isSprinting; // veya MovementController.IsSprinting()
        public bool IsCrouching() => playerData.isCrouching; // veya MovementController.IsCrouching()
        public bool IsGrounded() => playerData.isGrounded;   // veya MovementController.IsGrounded()
        #endregion
    }
}