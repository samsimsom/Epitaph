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
        [SerializeField] private float colorTransitionTime = 0.5f; // in game hours

        private GameTime _gameTime;

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
        }

        private void Update()
        {
            if (_gameTime != null && sunTransform != null)
            {
                UpdateSunPosition();
            }
        }

        private void UpdateSunPosition()
        {
            var month = _gameTime.GameMonth;
            var hour = _gameTime.GameHour;
            var minute = _gameTime.GameMinute;
            var timeOfDay = hour + (minute / 60f);

            // Get sunrise and sunset times for current month (1-based index)
            var (sunrise, sunset) = _sunriseSunsetData[month - 1];

            // Calculate sun rotation based on time of day
            float rotationY;  // East to West rotation
            float rotationX;  // Height rotation
            
            if (timeOfDay > sunrise && timeOfDay < sunset)
            {
                // Daytime - calculate position
                var dayProgress = (timeOfDay - sunrise) / (sunset - sunrise);
                rotationY = Mathf.Lerp(0, 180, dayProgress);
                
                // For height, peak at noon
                var midDay = sunrise + (sunset - sunrise) / 2;
                var heightProgress = 1 - Mathf.Abs((timeOfDay - midDay) / ((sunset - sunrise) / 2));
                rotationX = Mathf.Lerp(0, maxHeight, heightProgress);
            }
            else
            {
                // Nighttime
                if (timeOfDay < sunrise)
                {
                    // Before sunrise
                    var nightProgress = timeOfDay / sunrise;
                    rotationY = Mathf.Lerp(-180, 0, nightProgress);
                }
                else
                {
                    // After sunset
                    var nightProgress = (timeOfDay - sunset) / (24 + sunrise - sunset);
                    rotationY = Mathf.Lerp(180, 360, nightProgress);
                }
                
                // Keep sun below horizon at night
                rotationX = minHeight;
            }
            
            // Apply rotation
            sunTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            
            // Update sun color and intensity if light component is available
            if (sunLight != null)
            {
                UpdateSunLight(timeOfDay, sunrise, sunset);
            }
        }

        private void UpdateSunLight(float timeOfDay, float sunrise, float sunset)
        {
            // Calculate transition periods
            var sunriseStart = sunrise - colorTransitionTime;
            var sunriseEnd = sunrise + colorTransitionTime;
            var sunsetStart = sunset - colorTransitionTime;
            var sunsetEnd = sunset + colorTransitionTime;
            
            Color targetColor;
            float targetIntensity;
            
            // Determine color based on time
            if (timeOfDay < sunriseStart || timeOfDay > sunsetEnd)
            {
                // Night
                targetColor = nightColor;
                targetIntensity = 0.1f;
            }
            else if (timeOfDay >= sunriseStart && timeOfDay <= sunriseEnd)
            {
                // Sunrise transition
                var t = Mathf.InverseLerp(sunriseStart, sunriseEnd, timeOfDay);
                targetColor = Color.Lerp(nightColor, sunriseColor, t);
                targetIntensity = Mathf.Lerp(0.1f, 0.8f, t);
            }
            else if (timeOfDay >= sunsetStart && timeOfDay <= sunsetEnd)
            {
                // Sunset transition
                var t = Mathf.InverseLerp(sunsetStart, sunsetEnd, timeOfDay);
                targetColor = Color.Lerp(sunsetColor, nightColor, t);
                targetIntensity = Mathf.Lerp(0.8f, 0.1f, t);
            }
            else if (timeOfDay > sunriseEnd && timeOfDay < (sunrise + 2f))
            {
                // Early morning
                var t = Mathf.InverseLerp(sunriseEnd, sunrise + 2f, timeOfDay);
                targetColor = Color.Lerp(sunriseColor, daytimeColor, t);
                targetIntensity = Mathf.Lerp(0.8f, 1.0f, t);
            }
            else if (timeOfDay > (sunset - 2f) && timeOfDay < sunsetStart)
            {
                // Late afternoon
                var t = Mathf.InverseLerp(sunset - 2f, sunsetStart, timeOfDay);
                targetColor = Color.Lerp(daytimeColor, sunsetColor, t);
                targetIntensity = 0.8f;
            }
            else
            {
                // Daytime
                targetColor = daytimeColor;
                targetIntensity = 2.0f;
            }
            
            // Apply color and intensity
            sunLight.color = targetColor;
            sunLight.intensity = targetIntensity;
        }
    }
}