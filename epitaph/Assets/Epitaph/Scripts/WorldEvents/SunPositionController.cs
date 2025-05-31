using Epitaph.Scripts.GameTimeManager;
using UnityEngine;
using System;

namespace Epitaph.Scripts.WorldEvents
{
    public class SunPositionController : MonoBehaviour
    {
        [SerializeField] private Transform sunTransform;
        [SerializeField] private float maxHeight = 60f; // Maximum height angle of sun (at noon)
        [SerializeField] private float minHeight = -20f; // Minimum height angle of sun (at night)

        [Header("Sun Color")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Color daytimeColor = Color.white;
        [SerializeField] private Color sunriseColor = new Color(1f, 0.5f, 0.3f);
        [SerializeField] private Color sunsetColor = new Color(1f, 0.3f, 0.2f);
        [SerializeField] private Color nightColor = new Color(0.2f, 0.2f, 0.4f);
        [SerializeField] private float colorTransitionTime = 1.0f; // in game hours

        [Header("Intensity Multipliers")]
        [SerializeField] private float sunLightIntensityMultiplier = 1.0f;
        [SerializeField] private float ambientIntensityMultiplier = 1.0f;
        [SerializeField] private float maxSunLightIntensity = 1.0f;
        [SerializeField] private float maxAmbientIntensity = 0.3f;

        [Header("Smooth Transitions")]
        [SerializeField] private float positionSmoothness = 2f;
        [SerializeField] private float colorSmoothness = 1f;
        [SerializeField] private AnimationCurve sunHeightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private GameTime _gameTime;
        private Vector3 _targetRotation;
        private Color _targetColor;
        private float _targetIntensity;
        
        // Event for notifying other systems when sun data changes
        public static event Action<float, float, float> OnSunDataUpdated; // timeOfDay, sunrise, sunset

        // Sunrise and sunset data by month (24-hour format)
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
            
            if (sunTransform == null)
            {
                Debug.LogError("Sun Transform not assigned to SunPositionController");
            }

            if (sunLight == null && TryGetComponent(out Light sunLightComponent))
            {
                sunLight = sunLightComponent;
            }

            // Initialize target values
            _targetRotation = sunTransform != null ? sunTransform.eulerAngles : Vector3.zero;
            _targetColor = daytimeColor;
            _targetIntensity = maxSunLightIntensity;
        }

        private void Update()
        {
            if (_gameTime != null && sunTransform != null)
            {
                UpdateSunSystem();
            }
        }

        private void UpdateSunSystem()
        {
            var month = _gameTime.GameMonth;
            var timeOfDay = GetCurrentTimeOfDay();

            // Get sunrise and sunset times for current month (1-based index)
            var (sunrise, sunset) = _sunriseSunsetData[Mathf.Clamp(month - 1, 0, 11)];

            // Notify other systems about sun data update
            OnSunDataUpdated?.Invoke(timeOfDay, sunrise, sunset);

            // Calculate target sun position
            CalculateTargetSunPosition(timeOfDay, sunrise, sunset);

            // Calculate target sun lighting
            CalculateTargetSunLighting(timeOfDay, sunrise, sunset);

            // Apply smooth transitions
            ApplySmoothTransitions();
        }

        private float GetCurrentTimeOfDay()
        {
            return _gameTime.GameHour + (_gameTime.GameMinute / 60f) + (_gameTime.GameSecond / 3600f);
        }

        private void CalculateTargetSunPosition(float timeOfDay, float sunrise, float sunset)
        {
            float rotationY;  // East to West rotation (0° = East, 90° = South, 180° = West)
            float rotationX;  // Height rotation

            // Calculate day length
            var dayLength = sunset - sunrise;
            
            if (timeOfDay >= sunrise && timeOfDay <= sunset)
            {
                // Daytime - sun moves from east to west
                var dayProgress = (timeOfDay - sunrise) / dayLength;
                
                // Smooth Y rotation from -90° (east) to 270° (west)
                rotationY = Mathf.Lerp(-90f, 270f, dayProgress);
                
                // Calculate height using a curve for more realistic sun arc
                var heightProgress = sunHeightCurve.Evaluate(Mathf.Sin(dayProgress * Mathf.PI));
                rotationX = Mathf.Lerp(minHeight, maxHeight, heightProgress);
            }
            else
            {
                // Nighttime - sun continues its path underground
                var nightLength = 24f - dayLength;
                float nightProgress;
                
                if (timeOfDay < sunrise)
                {
                    // Before sunrise (early morning)
                    nightProgress = (timeOfDay + (24f - sunset)) / nightLength;
                }
                else
                {
                    // After sunset (evening/night)
                    nightProgress = (timeOfDay - sunset) / nightLength;
                }
                
                // Continue rotation underground
                rotationY = Mathf.Lerp(270f, 450f, nightProgress); // 450° = 90° (next day's east)
                if (rotationY >= 360f) rotationY -= 360f;
                
                // Keep sun below horizon during night
                rotationX = Mathf.Lerp(minHeight, minHeight - 20f, Mathf.Sin(nightProgress * Mathf.PI));
            }

            _targetRotation = new Vector3(rotationX, rotationY, 0);
        }

        private void CalculateTargetSunLighting(float timeOfDay, float sunrise, float sunset)
        {
            // Define transition periods
            var sunriseStart = sunrise - colorTransitionTime;
            var sunriseEnd = sunrise + colorTransitionTime;
            var sunsetStart = sunset - colorTransitionTime;
            var sunsetEnd = sunset + colorTransitionTime;
            
            Color targetColor;
            float targetIntensity;
            
            if (timeOfDay < sunriseStart || timeOfDay > sunsetEnd)
            {
                // Deep night
                targetColor = nightColor;
                targetIntensity = 0.05f;
            }
            else if (timeOfDay >= sunriseStart && timeOfDay <= sunriseEnd)
            {
                // Sunrise transition
                var t = Mathf.InverseLerp(sunriseStart, sunriseEnd, timeOfDay);
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth transition curve
                
                if (t < 0.5f)
                {
                    // Night to sunrise colors
                    targetColor = Color.Lerp(nightColor, sunriseColor, t * 2f);
                    targetIntensity = Mathf.Lerp(0.05f, 0.6f, t * 2f);
                }
                else
                {
                    // Sunrise to day colors
                    targetColor = Color.Lerp(sunriseColor, daytimeColor, (t - 0.5f) * 2f);
                    targetIntensity = Mathf.Lerp(0.6f, maxSunLightIntensity, (t - 0.5f) * 2f);
                }
            }
            else if (timeOfDay >= sunsetStart && timeOfDay <= sunsetEnd)
            {
                // Sunset transition
                var t = Mathf.InverseLerp(sunsetStart, sunsetEnd, timeOfDay);
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth transition curve
                
                if (t < 0.5f)
                {
                    // Day to sunset colors
                    targetColor = Color.Lerp(daytimeColor, sunsetColor, t * 2f);
                    targetIntensity = Mathf.Lerp(maxSunLightIntensity, 0.6f, t * 2f);
                }
                else
                {
                    // Sunset to night colors
                    targetColor = Color.Lerp(sunsetColor, nightColor, (t - 0.5f) * 2f);
                    targetIntensity = Mathf.Lerp(0.6f, 0.05f, (t - 0.5f) * 2f);
                }
            }
            else if (timeOfDay > sunriseEnd && timeOfDay < sunsetStart)
            {
                // Full daytime
                targetColor = daytimeColor;
                targetIntensity = maxSunLightIntensity;
            }
            else
            {
                // Fallback to night
                targetColor = nightColor;
                targetIntensity = 0.05f;
            }
            
            _targetColor = targetColor;
            _targetIntensity = targetIntensity;
        }

        private void ApplySmoothTransitions()
        {
            // Smooth sun position transition
            var currentRotation = sunTransform.eulerAngles;
            
            // Handle angle wrapping for smooth rotation
            var deltaY = Mathf.DeltaAngle(currentRotation.y, _targetRotation.y);
            var targetY = currentRotation.y + deltaY;
            
            var smoothRotation = new Vector3(
                Mathf.LerpAngle(currentRotation.x, _targetRotation.x, Time.deltaTime * positionSmoothness),
                Mathf.LerpAngle(currentRotation.y, targetY, Time.deltaTime * positionSmoothness),
                0f
            );
            
            sunTransform.rotation = Quaternion.Euler(smoothRotation);

            // Smooth lighting transition
            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(sunLight.color, _targetColor, Time.deltaTime * colorSmoothness);
                
                // Apply sun light intensity with multiplier
                var finalSunIntensity = _targetIntensity * sunLightIntensityMultiplier;
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, finalSunIntensity, Time.deltaTime * colorSmoothness);
                
                // Update ambient lighting smoothly with multiplier
                var baseAmbientTarget = _targetIntensity > 0.1f ? _targetIntensity * 0.3f : 0.05f;
                var finalAmbientTarget = Mathf.Min(baseAmbientTarget * ambientIntensityMultiplier, maxAmbientIntensity);
                
                RenderSettings.ambientIntensity = Mathf.Lerp(
                    RenderSettings.ambientIntensity, 
                    finalAmbientTarget, 
                    Time.deltaTime * colorSmoothness * 0.5f
                );
            }
        }

        // Debug methods
        [ContextMenu("Debug Current Sun Info")]
        private void DebugCurrentSunInfo()
        {
            if (_gameTime == null) return;
            
            var timeOfDay = GetCurrentTimeOfDay();
            var (sunrise, sunset) = _sunriseSunsetData[_gameTime.GameMonth - 1];
            
            Debug.Log($"Current Time: {timeOfDay:F2}h");
            Debug.Log($"Sunrise: {sunrise:F2}h, Sunset: {sunset:F2}h");
            Debug.Log($"Target Rotation: {_targetRotation}");
            Debug.Log($"Target Color: {_targetColor}, Intensity: {_targetIntensity:F2}");
            Debug.Log($"Final Sun Intensity: {_targetIntensity * sunLightIntensityMultiplier:F2}");
            Debug.Log($"Final Ambient Intensity: {RenderSettings.ambientIntensity:F2}");
        }

        // Public methods for external control
        public void SetTimeSpeedMultiplier(float multiplier)
        {
            positionSmoothness = Mathf.Max(0.1f, 2f * multiplier);
            colorSmoothness = Mathf.Max(0.1f, 1f * multiplier);
        }

        public (float sunrise, float sunset) GetCurrentSunriseSunset()
        {
            if (_gameTime == null) return (6f, 18f);
            return _sunriseSunsetData[Mathf.Clamp(_gameTime.GameMonth - 1, 0, 11)];
        }
        
        // Get current time data for synchronization
        public (float timeOfDay, float sunrise, float sunset) GetCurrentTimeData()
        {
            if (_gameTime == null) return (12f, 6f, 18f);
            
            var timeOfDay = GetCurrentTimeOfDay();
            var (sunrise, sunset) = GetCurrentSunriseSunset();
            return (timeOfDay, sunrise, sunset);
        }

        // Public methods to control multipliers at runtime
        public void SetSunLightIntensityMultiplier(float multiplier)
        {
            sunLightIntensityMultiplier = Mathf.Max(0f, multiplier);
        }

        public void SetAmbientIntensityMultiplier(float multiplier)
        {
            ambientIntensityMultiplier = Mathf.Max(0f, multiplier);
        }

        public void SetMaxIntensities(float maxSunLight, float maxAmbient)
        {
            maxSunLightIntensity = Mathf.Max(0f, maxSunLight);
            maxAmbientIntensity = Mathf.Max(0f, maxAmbient);
        }
    }
}