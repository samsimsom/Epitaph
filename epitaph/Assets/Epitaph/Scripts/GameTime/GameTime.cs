using UnityEngine;

namespace Epitaph.Scripts.GameTime
{
    public class GameTime : MonoBehaviour
    {
        // 120 dakika = 1 oyun günü
        [HideInInspector] public float realSecondsPerGameDay = 60f * 120f;
        [HideInInspector] public float elapsedGameSeconds;

        // Zaman başlangıcı: 9:30
        private const int StartHour = 9;
        private const int StartMinute = 30;
        private const int SecondsPerMinute = 60;
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int DaysPerMonth = 30;
        private const int MonthsPerYear = 12;

        // Oyun takvimi başlangıcı
        [HideInInspector] public int startYear = 1984;
        [HideInInspector] public int startMonth = 12;
        [HideInInspector] public int startDay = 15;

        private int StartTotalDays => GetStartTotalDays();

        // Toplam geçen oyun günü
        private int TotalGameDays => (int)(elapsedGameSeconds / (HoursPerDay * MinutesPerHour * SecondsPerMinute));

        public int GameYear
        {
            get
            {
                var days = StartTotalDays + TotalGameDays;
                return days / (MonthsPerYear * DaysPerMonth);
            }
        }
        public int GameMonth
        {
            get
            {
                var days = StartTotalDays + TotalGameDays;
                return (days / DaysPerMonth) % MonthsPerYear + 1;
            }
        }
        public int GameDay
        {
            get
            {
                var days = StartTotalDays + TotalGameDays;
                return (days % DaysPerMonth) + 1;
            }
        }
        
        public int GameHour
        {
            get
            {
                var totalSecondsToday = (int)(elapsedGameSeconds % (HoursPerDay * MinutesPerHour * SecondsPerMinute));
                return (totalSecondsToday / (MinutesPerHour * SecondsPerMinute)) % HoursPerDay;
            }
        }
        public int GameMinute
        {
            get
            {
                var totalSecondsToday = (int)(elapsedGameSeconds % (HoursPerDay * MinutesPerHour * SecondsPerMinute));
                var minutes = (totalSecondsToday / SecondsPerMinute) % MinutesPerHour;
                return minutes;
            }
        }
        
        public int GameSecond
        {
            get
            {
                var totalSecondsToday = (int)(elapsedGameSeconds % (HoursPerDay * MinutesPerHour * SecondsPerMinute));
                return totalSecondsToday % SecondsPerMinute;
            }
        }
        
        [Header("Current Time (Inspector)")]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string gameDate;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string gameClock;

        private void Start()
        {
            elapsedGameSeconds = StartHour * MinutesPerHour * SecondsPerMinute + StartMinute * SecondsPerMinute;
        }

        private void Update()
        {
            elapsedGameSeconds += Time.deltaTime * (HoursPerDay * MinutesPerHour * SecondsPerMinute) / realSecondsPerGameDay;
            UpdateInspectorValues();
        }
        
        private int GetStartTotalDays()
        {
            return (startYear * MonthsPerYear * DaysPerMonth) + ((startMonth - 1) * DaysPerMonth) + (startDay - 1);
        }

        private void UpdateInspectorValues()
        {
            gameDate = $"{GameDay:00}/{GameMonth:00}/{GameYear:0000}";
            gameClock = $"{GameHour:00}:{GameMinute:00}:{GameSecond:00}";
        }

        /// <summary>
        /// Oyunda zaman atlatır.
        /// </summary>
        /// <param name="hours">İleri atlanacak saat miktarı</param>
        public void SkipTime(float hours)
        {
            elapsedGameSeconds += hours * SecondsPerMinute * MinutesPerHour;
        }
    }
}