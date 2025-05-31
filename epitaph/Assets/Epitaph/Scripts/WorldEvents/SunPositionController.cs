using Epitaph.Scripts.GameTimeManager;
using UnityEngine;

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

        [Header("Smooth Transitions")]
        [SerializeField] private float positionSmoothness = 2f;
        [SerializeField] private float colorSmoothness = 1f;
        [SerializeField] private AnimationCurve sunHeightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private GameTime _gameTime;
        private Vector3 _targetRotation;
        private Color _targetColor;
        private float _targetIntensity;

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
            _targetIntensity = 1f;
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
            float dayLength = sunset - sunrise;
            
            if (timeOfDay >= sunrise && timeOfDay <= sunset)
            {
                // Daytime - sun moves from east to west
                float dayProgress = (timeOfDay - sunrise) / dayLength;
                
                // Smooth Y rotation from -90° (east) to 270° (west)
                rotationY = Mathf.Lerp(-90f, 270f, dayProgress);
                
                // Calculate height using a curve for more realistic sun arc
                float heightProgress = sunHeightCurve.Evaluate(Mathf.Sin(dayProgress * Mathf.PI));
                rotationX = Mathf.Lerp(minHeight, maxHeight, heightProgress);
            }
            else
            {
                // Nighttime - sun continues its path underground
                float nightLength = 24f - dayLength;
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
            float sunriseStart = sunrise - colorTransitionTime;
            float sunriseEnd = sunrise + colorTransitionTime;
            float sunsetStart = sunset - colorTransitionTime;
            float sunsetEnd = sunset + colorTransitionTime;
            
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
                float t = Mathf.InverseLerp(sunriseStart, sunriseEnd, timeOfDay);
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
                    targetIntensity = Mathf.Lerp(0.6f, 1.0f, (t - 0.5f) * 2f);
                }
            }
            else if (timeOfDay >= sunsetStart && timeOfDay <= sunsetEnd)
            {
                // Sunset transition
                float t = Mathf.InverseLerp(sunsetStart, sunsetEnd, timeOfDay);
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth transition curve
                
                if (t < 0.5f)
                {
                    // Day to sunset colors
                    targetColor = Color.Lerp(daytimeColor, sunsetColor, t * 2f);
                    targetIntensity = Mathf.Lerp(1.0f, 0.6f, t * 2f);
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
                targetIntensity = 1.0f;
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
            Vector3 currentRotation = sunTransform.eulerAngles;
            
            // Handle angle wrapping for smooth rotation
            float deltaY = Mathf.DeltaAngle(currentRotation.y, _targetRotation.y);
            float targetY = currentRotation.y + deltaY;
            
            Vector3 smoothRotation = new Vector3(
                Mathf.LerpAngle(currentRotation.x, _targetRotation.x, Time.deltaTime * positionSmoothness),
                Mathf.LerpAngle(currentRotation.y, targetY, Time.deltaTime * positionSmoothness),
                0f
            );
            
            sunTransform.rotation = Quaternion.Euler(smoothRotation);

            // Smooth lighting transition
            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(sunLight.color, _targetColor, Time.deltaTime * colorSmoothness);
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, _targetIntensity, Time.deltaTime * colorSmoothness);
                
                // Update ambient lighting smoothly
                float targetAmbient = _targetIntensity > 0.1f ? _targetIntensity * 0.3f : 0.05f;
                RenderSettings.ambientIntensity = Mathf.Lerp(
                    RenderSettings.ambientIntensity, 
                    targetAmbient, 
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
    }
}