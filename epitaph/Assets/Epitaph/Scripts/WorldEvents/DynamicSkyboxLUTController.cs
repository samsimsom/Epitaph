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

        // Sunrise and sunset data by month (24-hour format) - copied from SunPositionController
        private readonly (float sunrise, float sunset)[] _sunriseSunsetData = new[]
        {
            (8.25f, 17.5f),    // January
            (7.75f, 18.0f),    // February
            (7.0f, 18.5f),     // March
            (6.33f, 19.0f),    // April
            (5.67f, 19.5f),    // May
            (5.5f, 20.0f),     // June
            (5.75f, 20.17f),   // July
            (6.25f, 19.75f),   // August
            (6.75f, 19.0f),    // September
            (7.25f, 18.25f),   // October
            (7.75f, 17.5f),    // November
            (8.17f, 17.25f)    // December
        };
        
        private void Awake()
        {
            _gameTime = GameTime.Instance;
            
            // Post-processing volume'ü bulup ColorLookup komponentini al
            if (postProcessVolume != null && postProcessVolume.Profile != null)
            {
                postProcessVolume.Profile.TryGet(out _colorLookup);
            }
            
            ValidateComponents();
        }
        
        private void Start()
        {
            // İlk skybox ve LUT'u ayarla
            UpdateSkyboxAndLut();
        }
#if true
        
        private void Update()
        {
            if (_gameTime != null)
            {
                UpdateSkyboxAndLut();
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
            
            // Get sunrise and sunset times for current month
            var (sunrise, sunset) = _sunriseSunsetData[month - 1];
            
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
            
            // Store current skybox
            // var previousSkybox = RenderSettings.skybox;
            
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
        
        // Debug method to check current time period
        [ContextMenu("Debug Current Time Period")]
        private void DebugCurrentTimePeriod()
        {
            if (_gameTime == null) return;
            
            var timeOfDay = _gameTime.GameHour + (_gameTime.GameMinute / 60f);
            var (sunrise, sunset) = _sunriseSunsetData[_gameTime.GameMonth - 1];
            var (skybox, lut) = DetermineTargetMaterials(timeOfDay, sunrise, sunset);
            
            Debug.Log($"Current Time: {timeOfDay:F2}h, Sunrise: {sunrise:F2}h, Sunset: {sunset:F2}h");
            Debug.Log($"Target Skybox: {skybox.name}, Target LUT: {lut.name}");
        }
#endif
    }
}