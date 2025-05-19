using UnityEngine;

namespace Epitaph.Scripts.GameTimeManager
{
    public class GameTimeEventListener : MonoBehaviour
    {
        [SerializeField] private GameTime gameTime;

        private void OnEnable()
        {
            if (gameTime == null)
            {
                Debug.LogError("GameTime reference not set!", this);
                return;
            }

            gameTime.OnDayPassed += HandleDayPassed;
            gameTime.OnMonthPassed += HandleMonthPassed;
            gameTime.OnYearPassed += HandleYearPassed;
            gameTime.OnSeasonChanged += HandleSeasonChanged;
        }

        private void OnDisable()
        {
            if (gameTime == null) return;

            gameTime.OnDayPassed -= HandleDayPassed;
            gameTime.OnMonthPassed -= HandleMonthPassed;
            gameTime.OnYearPassed -= HandleYearPassed;
            gameTime.OnSeasonChanged -= HandleSeasonChanged;
        }

        private void HandleDayPassed()
        {
            Debug.LogWarning("[GameTimeEventListener] Yeni bir gün başladı!");
        }

        private void HandleMonthPassed()
        {
            Debug.LogWarning("[GameTimeEventListener] Yeni bir ay başladı!");
        }

        private void HandleYearPassed()
        {
            Debug.LogWarning("[GameTimeEventListener] Yeni bir yıl başladı!");
        }

        private void HandleSeasonChanged(GameTime.Season oldSeason, GameTime.Season newSeason)
        {
            Debug.LogWarning($"[GameTimeEventListener] Mevsim değişti! {oldSeason} -> {newSeason}");
        }
    }
}