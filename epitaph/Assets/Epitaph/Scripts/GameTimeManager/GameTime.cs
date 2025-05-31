using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Epitaph.Scripts.GameTimeManager
{
    [DefaultExecutionOrder(-100)]
    public class GameTime : MonoBehaviour
    {
        // Singleton instance
        public static GameTime Instance { get; private set; }
        
        #region Time Constants
        // Real World Time Constants
        private const int SecondsPerMinute = 60;
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int DaysPerMonth = 30;
        private const int MonthsPerYear = 12;
        #endregion

        #region Public Properties
        [Header("Game Speed")]
        [Tooltip("Real seconds needed for one game day to pass")]
        public float realSecondsPerGameDay = 60f * 120f; // 120 minutes = 1 game day
        
        [Header("Starting Time")]
        [Tooltip("Starting hour of the game (0-23)")]
        [Range(0, 23)]
        public int startHour = 9;
        
        [Tooltip("Starting minute of the game (0-59)")]
        [Range(0, 59)]
        public int startMinute = 30;
        
        [Header("Elapsed Time")]
        public float elapsedGameSeconds;
        public float startElapsedGameSeconds;
        
        [Header("Starting Calendar Date")]
        [Tooltip("Year the game calendar starts")]
        public int startYear = 1984;
        
        [Tooltip("Month the game calendar starts (1-12)")]
        [Range(1, 12)]
        public int startMonth = 12;
        
        [Tooltip("Day the game calendar starts (1-30)")]
        [Range(1, 30)]
        public int startDay = 15;
        #endregion

        #region Time Properties
        private int StartTotalDays => GetStartTotalDays();
        private int TotalGameDays => (int)(elapsedGameSeconds / (HoursPerDay * MinutesPerHour * SecondsPerMinute));
        
        public int GameYear => (StartTotalDays + TotalGameDays) / (MonthsPerYear * DaysPerMonth);
        
        public int GameMonth => ((StartTotalDays + TotalGameDays) / DaysPerMonth) % MonthsPerYear + 1;
        
        public int GameDay => ((StartTotalDays + TotalGameDays) % DaysPerMonth) + 1;
        
        public int GameHour => (GetTotalSecondsToday() / (MinutesPerHour * SecondsPerMinute)) % HoursPerDay;
        
        public int GameMinute => (GetTotalSecondsToday() / SecondsPerMinute) % MinutesPerHour;
        
        public int GameSecond => GetTotalSecondsToday() % SecondsPerMinute;
        #endregion

        #region Debugging Data
        [Header("Current Time (Inspector)")]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string gameDate;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string gameClock;
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string startElapsedTime;
        #endregion

        #region Events
        public event Action OnDayPassed;
        public event Action OnMonthPassed;
        public event Action OnYearPassed;
        public event Action<Season, Season> OnSeasonChanged;
        public event Action<float> OnTimeSkipped;
        #endregion

        #region Private Fields
        private int _lastGameDay;
        private int _lastGameMonth;
        private int _lastGameYear;
        private Season _lastSeason;
        private CancellationTokenSource _cts;
        #endregion

        #region Enums
        public enum Season
        {
            Winter,
            Spring,
            Summer,
            Fall
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton pattern implementation
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Found more than one GameTime instance in the scene. " +
                                 "Destroying this one.");
                Destroy(gameObject);
                return;
            }
        
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        
            // Original initialization code
            InitializeTime();
        }

        private void Start()
        {
            // InitializeTime();
            StartTimeUpdateLoop().Forget();
        }

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
        #endregion

        #region Time Initialization
        private void InitializeTime()
        {
            startElapsedGameSeconds = 0f;
            elapsedGameSeconds = startHour * MinutesPerHour * SecondsPerMinute + startMinute * SecondsPerMinute;
            
            // Initialize tracking values
            _lastGameDay = GameDay;
            _lastGameMonth = GameMonth;
            _lastGameYear = GameYear;
            _lastSeason = GetSeasonFromMonth(GameMonth);
        }
        #endregion

        #region Time Updates
        private async UniTaskVoid StartTimeUpdateLoop()
        {
            while (_cts != null && !_cts.IsCancellationRequested)
            {
                await UpdateGameTimeAsync();
                await UpdateInspectorValuesAsync();
                await CheckTimeEventsAsync();
                
                // Optimize by avoiding per-frame execution
                await UniTask.Yield();
            }
        }
        
        private async UniTask UpdateGameTimeAsync()
        {
            float timeScale = (HoursPerDay * MinutesPerHour * SecondsPerMinute) / realSecondsPerGameDay;
            startElapsedGameSeconds += Time.deltaTime * timeScale;
            elapsedGameSeconds += Time.deltaTime * timeScale;
            
            await UniTask.CompletedTask;
        }
        
        private async UniTask CheckTimeEventsAsync()
        {
            // Check for day change
            if (GameDay != _lastGameDay)
            {
                _lastGameDay = GameDay;
                OnDayPassed?.Invoke();
            }

            // Check for month change
            if (GameMonth != _lastGameMonth)
            {
                _lastGameMonth = GameMonth;
                OnMonthPassed?.Invoke();
            }

            // Check for year change
            if (GameYear != _lastGameYear)
            {
                _lastGameYear = GameYear;
                OnYearPassed?.Invoke();
            }
            
            // Check for season change
            var currentSeason = GetSeasonFromMonth(GameMonth);
            if (currentSeason != _lastSeason)
            {
                var oldSeason = _lastSeason;
                _lastSeason = currentSeason;
                OnSeasonChanged?.Invoke(oldSeason, currentSeason);
            }
            
            await UniTask.CompletedTask;
        }
        #endregion

        #region Helper Methods
        private int GetTotalSecondsToday()
        {
            return (int)(elapsedGameSeconds % (HoursPerDay * MinutesPerHour * SecondsPerMinute));
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

        private async UniTask UpdateInspectorValuesAsync()
        {
            gameDate = $"{GameDay:00}/{GameMonth:00}/{GameYear:0000}";
            gameClock = $"{GameHour:00}:{GameMinute:00}:{GameSecond:00}";
            startElapsedTime = await GetElapsedStringAsync();
        }

        /// <summary>
        /// Returns a formatted string of time elapsed since game start
        /// </summary>
        private async UniTask<string> GetElapsedStringAsync()
        {
            var totalSeconds = (int)startElapsedGameSeconds;

            var seconds = totalSeconds % 60;
            var totalMinutes = totalSeconds / 60;
            var minutes = totalMinutes % 60;
            var totalHours = totalMinutes / 60;
            var hours = totalHours % 24;
            var totalDays = totalHours / 24;
            var days = totalDays % 30;
            var totalMonths = totalDays / 30;
            var months = totalMonths % 12;
            var years = totalMonths / 12;

            // Daha karmaşık bir hesaplama olduğunda burada işlemleri async yapmak faydalı olabilir
            await UniTask.CompletedTask;
            
            return $"{years} yıl, {months} ay, {days} gün, {hours} saat, {minutes} dakika, {seconds} saniye";
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Advances game time by specified hours
        /// </summary>
        /// <param name="hours">Hours to skip forward</param>
        public async UniTask SkipTimeAsync(float hours)
        {
            var oldSeconds = elapsedGameSeconds;
            elapsedGameSeconds += hours * SecondsPerMinute * MinutesPerHour;
            
            // Zaman atlamayı olaylar aracılığıyla bildir
            OnTimeSkipped?.Invoke(hours);

            // Ensure event checks are run after skipping time
            await CheckTimeEventsAsync();
            await UpdateInspectorValuesAsync();
        }
        
        /// <summary>
        /// Aktif oyun saniyesinin bitmesini bekler. 
        /// Bu metot çağrıldığında, mevcut GameSecond değeri değişene kadar bekler.
        /// Genellikle bir işlemi oyun zamanına göre, tam saniye aralıklarında başlatmak için kullanılır.
        /// </summary>
        /// <returns>Bir "game second" tamamlandığında tamamlanan bir UniTask.</returns>
        public async UniTask WaitForGameSecond(CancellationToken token)
        {
            var startSecond = GameSecond;
            while (GameSecond == startSecond)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        /// <summary>
        /// Aktif oyun dakikasinin bitmesini bekler. 
        /// Bu metot çağrıldığında, mevcut GameMinute değeri değişene kadar bekler.
        /// Genellikle bir işlemi oyun zamanına göre, tam dakika aralıklarında başlatmak için kullanılır.
        /// </summary>
        /// <returns>Bir "game minute" tamamlandığında tamamlanan bir UniTask.</returns>
        public async UniTask WaitForGameMinutes(CancellationToken token)
        {
            var startSecond = GameMinute;
            while (GameMinute == startSecond)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        #endregion
    }
}