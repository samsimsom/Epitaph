using System.Collections;
using Epitaph.Scripts.GameTimeManager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Epitaph.Scripts.WorldEvents
{
    public class DynamicSkyboxLutController : MonoBehaviour
    {
        [Header("Skybox Materials")]
        [SerializeField] private Material dayMaterial;
        [SerializeField] private Material nightMaterial;
        [SerializeField] private Material sunsetMaterial;
        
        [Header("LUT Textures")]
        [SerializeField] private Texture dayLut;
        [SerializeField] private Texture duskLut;
        [SerializeField] private Texture nightLut;
        
        [Header("Camera Post-Processing")]
        [SerializeField] private CinemachineVolumeSettings postProcessVolume;
        
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 0.5f; // in game hours
        [SerializeField] private bool smoothTransitions = true;
        
        private GameTime _gameTime;
        private ColorLookup _colorLookup;
        private Material _currentSkybox;
        private bool _isTransitioning;
        
        // Reference to SunPositionController for synchronized data
        private SunPositionController _sunController;

        private void Awake()
        {
            _gameTime = GameTime.Instance;
            
            // Find SunPositionController for data synchronization
            // _sunController = FindFirstObjectByType<SunPositionController>();
            _sunController = GetComponent<SunPositionController>();
            if (_sunController == null)
            {
                Debug.LogError("SunPositionController not found! Skybox controller requires it for synchronization.");
            }
            
            // Post-processing volume'ü bulup ColorLookup komponentini al
            if (postProcessVolume != null && postProcessVolume.Profile != null)
            {
                postProcessVolume.Profile.TryGet(out _colorLookup);
            }
            
            ValidateComponents();
        }
        
        private void Start()
        {
            // Subscribe to time change events instead of Update
            if (_gameTime != null)
            {
                // İlk skybox ve LUT'u ayarla
                UpdateSkyboxAndLut();
                
                // Subscribe to time change events (if available)
                // Note: This would require GameTime to have events
                StartCoroutine(SynchronizedUpdateCoroutine());
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events if implemented
        }
        
        // Synchronized update coroutine that runs after SunPositionController
        private IEnumerator SynchronizedUpdateCoroutine()
        {
            while (gameObject.activeInHierarchy)
            {
                // Wait for end of frame to ensure SunPositionController has updated
                yield return new WaitForEndOfFrame();
                
                if (_gameTime != null)
                {
                    UpdateSkyboxAndLut();
                }
                
                // Update every few frames instead of every frame for performance
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void ValidateComponents()
        {
            if (_gameTime == null)
                Debug.LogError("GameTime instance not found!");
                
            if (dayMaterial == null || nightMaterial == null || sunsetMaterial == null)
                Debug.LogError("One or more skybox materials are missing!");
                
            if (dayLut == null || duskLut == null || nightLut == null)
                Debug.LogError("One or more LUT textures are missing!");
                
            if (_colorLookup == null)
                Debug.LogError("ColorLookup component not found in post-processing volume!");
        }
        
        private void UpdateSkyboxAndLut()
        {
            var month = _gameTime.GameMonth;
            var hour = _gameTime.GameHour;
            var minute = _gameTime.GameMinute;
            var timeOfDay = hour + (minute / 60f);
            
            // Get sunrise and sunset times from SunPositionController for synchronization
            var (sunrise, sunset) = _sunController != null ? 
                _sunController.GetCurrentSunriseSunset() : 
                (8f, 18f); // fallback values
            
            // Determine which skybox and LUT to use based on time
            var (targetSkybox, targetLut) = DetermineTargetMaterials(timeOfDay, sunrise, sunset);
            
            // Apply skybox
            if (RenderSettings.skybox != targetSkybox)
            {
                if (smoothTransitions && !_isTransitioning)
                {
                    StartCoroutine(TransitionSkybox(targetSkybox));
                }
                else if (!smoothTransitions)
                {
                    RenderSettings.skybox = targetSkybox;
                    DynamicGI.UpdateEnvironment();
                }
            }
            
            // Apply LUT
            if (_colorLookup != null && _colorLookup.texture.value != targetLut)
            {
                _colorLookup.texture.value = targetLut;
            }
        }
        
        private (Material skybox, Texture lut) DetermineTargetMaterials(float timeOfDay, float sunrise, float sunset)
        {
            // Define transition periods
            var sunriseStart = sunrise - transitionDuration;
            var sunriseEnd = sunrise + transitionDuration;
            var sunsetStart = sunset - transitionDuration;
            var sunsetEnd = sunset + transitionDuration;
            
            Material targetSkybox;
            Texture targetLut;
            
            if (timeOfDay >= sunriseStart && timeOfDay <= sunriseEnd)
            {
                // Sunrise period - use sunset skybox and dusk LUT
                targetSkybox = sunsetMaterial;
                targetLut = duskLut;
            }
            else if (timeOfDay >= sunsetStart && timeOfDay <= sunsetEnd)
            {
                // Sunset period - use sunset skybox and dusk LUT
                targetSkybox = sunsetMaterial;
                targetLut = duskLut;
            }
            else if (timeOfDay > sunriseEnd && timeOfDay < sunsetStart)
            {
                // Daytime - use day skybox and day LUT
                targetSkybox = dayMaterial;
                targetLut = dayLut;
            }
            else
            {
                // Nighttime - use night skybox and night LUT
                targetSkybox = nightMaterial;
                targetLut = nightLut;
            }
            
            return (targetSkybox, targetLut);
        }
        
        private IEnumerator TransitionSkybox(Material targetSkybox)
        {
            _isTransitioning = true;
            
            // Create transition
            var transitionProgress = 0f;
            var transitionSpeed = 1f / (transitionDuration * 60f); // Convert game hours to real seconds
            
            while (transitionProgress < 1f)
            {
                transitionProgress += Time.deltaTime * transitionSpeed;
                transitionProgress = Mathf.Clamp01(transitionProgress);
                
                // For skybox transition, we'll just switch at halfway point
                // (Unity doesn't support skybox blending out of the box)
                if (transitionProgress >= 0.5f && RenderSettings.skybox != targetSkybox)
                {
                    RenderSettings.skybox = targetSkybox;
                    DynamicGI.UpdateEnvironment();
                }
                
                yield return null;
            }
            
            _isTransitioning = false;
        }
        
        // Public methods for manual control
        public void SetDaySkybox()
        {
            RenderSettings.skybox = dayMaterial;
            if (_colorLookup != null) _colorLookup.texture.value = dayLut;
            DynamicGI.UpdateEnvironment();
        }
        
        public void SetNightSkybox()
        {
            RenderSettings.skybox = nightMaterial;
            if (_colorLookup != null) _colorLookup.texture.value = nightLut;
            DynamicGI.UpdateEnvironment();
        }
        
        public void SetSunsetSkybox()
        {
            RenderSettings.skybox = sunsetMaterial;
            if (_colorLookup != null) _colorLookup.texture.value = duskLut;
            DynamicGI.UpdateEnvironment();
        }
        
        // Method to force synchronization with SunPositionController
        public void ForceSynchronize()
        {
            if (_sunController != null)
            {
                UpdateSkyboxAndLut();
            }
        }
        
        // Debug method to check current time period
        [ContextMenu("Debug Current Time Period")]
        private void DebugCurrentTimePeriod()
        {
            if (_gameTime == null) return;
            
            var timeOfDay = _gameTime.GameHour + (_gameTime.GameMinute / 60f);
            var (sunrise, sunset) = _sunController != null ? 
                _sunController.GetCurrentSunriseSunset() : 
                (8f, 18f);
            var (skybox, lut) = DetermineTargetMaterials(timeOfDay, sunrise, sunset);
            
            Debug.Log($"Current Time: {timeOfDay:F2}h, Sunrise: {sunrise:F2}h, Sunset: {sunset:F2}h");
            Debug.Log($"Target Skybox: {skybox.name}, Target LUT: {lut.name}");
            Debug.Log($"Synchronized with SunPositionController: {_sunController != null}");
        }
    }
}