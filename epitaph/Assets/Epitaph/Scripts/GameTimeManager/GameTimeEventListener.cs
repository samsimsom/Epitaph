using UnityEngine;

namespace Epitaph.Scripts.GameTimeManager
{
    public class GameTimeEventListener : MonoBehaviour
    {
        private void OnEnable()
        {
            GameTime.Instance.OnDayPassed += HandleDayPassed;
            GameTime.Instance.OnMonthPassed += HandleMonthPassed;
            GameTime.Instance.OnYearPassed += HandleYearPassed;
            GameTime.Instance.OnSeasonChanged += HandleSeasonChanged;
        }

        private void OnDisable()
        {
            GameTime.Instance.OnDayPassed -= HandleDayPassed;
            GameTime.Instance.OnMonthPassed -= HandleMonthPassed;
            GameTime.Instance.OnYearPassed -= HandleYearPassed;
            GameTime.Instance.OnSeasonChanged -= HandleSeasonChanged;
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