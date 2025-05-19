using Epitaph.Scripts.GameTimeManager;
using UnityEngine;

namespace Epitaph.Scripts.WorldEvents
{
    public class SkyboxController : MonoBehaviour
    {
        [Header("Cubemap Sources")]
        [SerializeField] private Cubemap dayCubemap;
        [SerializeField] private Cubemap nightCubemap;

        [Header("Blended Cubemap Shader & Transition")]
        [SerializeField] private Shader blendedCubemapShader; // Should point to "Skybox/BlendedCubemap"
        [SerializeField] private float transitionDuration = 1.5f;
        [SerializeField] private float skyboxRotationSpeed = 0.5f; // Deg/sec or can be lerped

        [Header("References")]
        [SerializeField] private SunPositionController sunPositionController;

        private Material _skyboxMaterial;
        private float _blend = 1f; // 1.0=day, 0.0=night
        private GameTime _gameTime;

        private (float sunrise, float sunset)[] _sunriseSunsetData = new (float, float)[12];

        private void Awake()
        {
            _gameTime = GameTime.Instance;

            // easy error check
            if (!dayCubemap || !nightCubemap)
                Debug.LogError("SkyboxController: Assign day/night Cubemap assets!");

            if (!blendedCubemapShader)
                blendedCubemapShader = Shader.Find("Skybox/BlendedCubemap");

            _skyboxMaterial = new Material(blendedCubemapShader);
            _skyboxMaterial.SetTexture("_Cubemap1", dayCubemap);
            _skyboxMaterial.SetTexture("_Cubemap2", nightCubemap);

            if (sunPositionController == null)
                sunPositionController = FindFirstObjectByType<SunPositionController>();

            CacheSunriseAndSunsetData();
            
            // İlk başta rotasyonu hemen uygula
            var initialRotY = 0f;
            if (sunPositionController && sunPositionController.transform)
                initialRotY = sunPositionController.transform.eulerAngles.y;

            _skyboxMaterial.SetFloat("_Rotation", initialRotY);

        }

        private void OnEnable()
        {
            if (_gameTime != null)
                _gameTime.OnSeasonChanged += (_, _) => CacheSunriseAndSunsetData();
        }
        private void OnDisable()
        {
            if (_gameTime != null)
                _gameTime.OnSeasonChanged -= (_, _) => CacheSunriseAndSunsetData();
        }

        private void Update()
        {
            UpdateSkyboxBlending();
            UpdateSkyboxRotation();
        }

        private void UpdateSkyboxBlending()
        {
            var month = Mathf.Clamp(_gameTime.GameMonth, 1, 12);
            float hour = _gameTime.GameHour;
            float minute = _gameTime.GameMinute;
            var timeOfDay = hour + (minute / 60f);

            var (sunrise, sunset) = _sunriseSunsetData[month - 1];
            var sunriseStart = sunrise - transitionDuration;
            var sunriseEnd = sunrise + transitionDuration;
            var sunsetStart = sunset - transitionDuration;
            var sunsetEnd = sunset + transitionDuration;

            // Transition blend
            if (timeOfDay >= sunriseStart && timeOfDay <= sunriseEnd)
            {
                _blend = Mathf.InverseLerp(sunriseStart, sunriseEnd, timeOfDay); // night->day
            }
            else if (timeOfDay >= sunsetStart && timeOfDay <= sunsetEnd)
            {
                _blend = Mathf.InverseLerp(sunsetEnd, sunsetStart, timeOfDay); // day->night
            }
            else if (timeOfDay > sunriseEnd && timeOfDay < sunsetStart)
            {
                _blend = 1f; // day
            }
            else
            {
                _blend = 0f; // night
            }

            // Cubemap'ler shader property'lerine once assign edilir
            _skyboxMaterial.SetTexture("_Cubemap1", dayCubemap);
            _skyboxMaterial.SetTexture("_Cubemap2", nightCubemap);
            _skyboxMaterial.SetFloat("_Blend", _blend);

            RenderSettings.skybox = _skyboxMaterial;
        }

        private void UpdateSkyboxRotation()
        {
            var targetRotY = 0f;
            if (sunPositionController && sunPositionController.transform)
                targetRotY = sunPositionController.transform.eulerAngles.y;
            else
                targetRotY = (_skyboxMaterial.GetFloat("_Rotation") + skyboxRotationSpeed * Time.deltaTime) % 360f;

            // Her karede doğrudan değeri uygula (ilk framede keskin geçiş sağlayacak)
            _skyboxMaterial.SetFloat("_Rotation", targetRotY);

            // var currentRot = _skyboxMaterial.GetFloat("_Rotation");
            // var newRot = Mathf.LerpAngle(currentRot, targetRotY, skyboxRotationSpeed * Time.deltaTime);
            // _skyboxMaterial.SetFloat("_Rotation", newRot);
        }

        private void CacheSunriseAndSunsetData()
        {
            // Example: can be replaced with a dynamic source
            _sunriseSunsetData = new[]
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
        }

        // To change cubemaps at runtime if needed:
        public void SetDaytimeCubemap(Cubemap newDay)
        {
            dayCubemap = newDay;
            _skyboxMaterial.SetTexture("_Cubemap1", dayCubemap);
        }
        public void SetNighttimeCubemap(Cubemap newNight)
        {
            nightCubemap = newNight;
            _skyboxMaterial.SetTexture("_Cubemap2", nightCubemap);
        }
    }
}