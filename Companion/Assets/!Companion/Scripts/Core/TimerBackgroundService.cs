using MyNativeAndroidNotify;
using UnityEngine;
using Zenject;

namespace Companion.Core
{
    /// <summary>
    /// Мост «фон ↔ возврат» для таймеров. При сворачивании планирует системный будильник
    /// (нативный плагин MyNativeAndroidNotify) на завершение каждого идущего таймера — он
    /// звенит, пока не выключишь, даже если приложение закрыто. При возврате снимает будильники,
    /// глушит звон и до-завершает таймеры, истёкшие в фоне (дальше работает обычный in-app сигнал).
    /// </summary>
    public class TimerBackgroundService : MonoBehaviour
    {
        [Inject] private TimerService _timers;

        private void Awake()
        {
            AlarmNotify.RequestNotificationPermission();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                // Уходим в фон: планируем будильник на остаток каждого таймера.
                foreach (var (timer, remaining) in _timers.GetRunning())
                {
                    if (remaining > 0)
                        AlarmNotify.Schedule(timer.id, remaining, "Таймер", $"«{timer.name}» — время вышло");
                }
            }
            else
            {
                // Вернулись: снимаем запланированное и глушим возможный звон.
                foreach (var (timer, _) in _timers.GetRunning())
                    AlarmNotify.Cancel(timer.id);

                AlarmNotify.StopRinging();

                // Истёкшие в фоне: показать попап (какой таймер сработал), но БЕЗ in-app звука
                // (нативный будильник уже отзвенел).
                _timers.CompleteElapsed();
            }
        }
    }
}
