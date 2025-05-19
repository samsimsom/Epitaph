using UnityEngine;

namespace Epitaph.Scripts.GameTime
{
    public class GameTime : MonoBehaviour
    {
        // 120 dakika = 1 oyun günü
        public float realSecondsPerGameDay = 60f * 120f;
        public float elapsedGameSeconds;

        public int GameDay => (int)(elapsedGameSeconds / (24f * 60f * 60f)) + 1;
        public int GameHour => (int)((elapsedGameSeconds / 3600f) % 24);
        public int GameMinute => (int)((elapsedGameSeconds / 60f) % 60);

        [Header("Current Time (Inspector)")]
        [SerializeField] private float inspectorGameSeconds;
        [SerializeField] private int inspectorGameDay;
        [SerializeField] private int inspectorGameHour;
        [SerializeField] private int inspectorGameMinute;

        private void Update()
        {
            elapsedGameSeconds += Time.deltaTime * (24f * 60f * 60f) / realSecondsPerGameDay;
            UpdateInspectorValues();
        }

        private void UpdateInspectorValues()
        {
            // Inspector'da göstermek için değerleri güncelle
            inspectorGameSeconds = elapsedGameSeconds;
            inspectorGameDay = GameDay;
            inspectorGameHour = GameHour;
            inspectorGameMinute = GameMinute;
        }
        
        /// <summary>
        /// Oyunda zaman atlatır.
        /// </summary>
        /// <param name="hours">İleri atlanacak saat miktarı</param>

        public void SkipTime(float hours)
        {
            elapsedGameSeconds += hours * 3600f;
        }

    }
}