using System;
using UnityEngine;

namespace Epitaph.Scripts.GameTime
{
    public class GameTime : MonoBehaviour
    {
        // 120 dakika = 1 oyun günü
        public float realSecondsPerGameDay = 60f * 120f;
        public float elapsedGameSeconds;

        // Zaman başlangıcı: 9:30
        private const int StartHour = 9;
        private const int StartMinute = 30;
        private const int SecondsPerMinute = 60;
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int DaysPerMonth = 30;
        private const int MonthsPerYear = 12;

        // Oyun takvimi başlangıcı
        public int startYear = 1984;
        public int startMonth = 12;
        public int startDay = 15;

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
        
        // Olaylar (event) ekleniyor
        public event Action OnDayPassed;
        public event Action OnMonthPassed;
        public event Action OnYearPassed;

        private int _lastGameDay;
        private int _lastGameMonth;
        private int _lastGameYear;

        public enum Season
        {
            Winter,
            Spring,
            Summer,
            Fall
        }

        public event Action<Season, Season> OnSeasonChanged;

        private Season _lastSeason;

        private void Start()
        {
            elapsedGameSeconds = StartHour * MinutesPerHour * SecondsPerMinute + StartMinute * SecondsPerMinute;
            
            // Başlangıç değerlerini al
            _lastGameDay = GameDay;
            _lastGameMonth = GameMonth;
            _lastGameYear = GameYear;
            
            _lastSeason = GetSeasonFromMonth(GameMonth);
        }

        private void Update()
        {
            elapsedGameSeconds += Time.deltaTime * (HoursPerDay * MinutesPerHour * SecondsPerMinute) / realSecondsPerGameDay;
            UpdateInspectorValues();
            
            // Geçişleri kontrol et
            if (GameDay != _lastGameDay)
            {
                _lastGameDay = GameDay;
                OnDayPassed?.Invoke();
            }

            if (GameMonth != _lastGameMonth)
            {
                _lastGameMonth = GameMonth;
                OnMonthPassed?.Invoke();
            }

            if (GameYear != _lastGameYear)
            {
                _lastGameYear = GameYear;
                OnYearPassed?.Invoke();
            }
            
            var currentSeason = GetSeasonFromMonth(GameMonth);
            if (currentSeason != _lastSeason)
            {
                var oldSeason = _lastSeason;
                _lastSeason = currentSeason;
                OnSeasonChanged?.Invoke(oldSeason, currentSeason);
            }
        }
        
        private int GetStartTotalDays()
        {
            return (startYear * MonthsPerYear * DaysPerMonth) + ((startMonth - 1) * DaysPerMonth) + (startDay - 1);
        }
        
        public Season GetSeasonFromMonth(int month)
        {
            if (month == 12 || month == 1 || month == 2)
                return Season.Winter;
            if (month >= 3 && month <= 5)
                return Season.Spring;
            if (month >= 6 && month <= 8)
                return Season.Summer;
            return Season.Fall;
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