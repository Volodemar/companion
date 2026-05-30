using UnityEngine;
using Zenject;

namespace Companion.Core
{
    /// <summary>
    /// Мост «фон ↔ возврат» для таймеров. При сворачивании приложения планирует
    /// системные уведомления на завершение каждого идущего таймера (ОС сработает даже
    /// если приложение закрыто). При возврате — снимает уведомления и до-завершает
    /// таймеры, чьё время истекло, пока приложение было в фоне.
    /// </summary>
    public class TimerBackgroundService : MonoBehaviour
    {
        [Inject] private TimerService _timers;
        [Inject] private NotificationService _notifications;

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                // Уходим в фон: планируем уведомления на остаток каждого таймера.
                _notifications.CancelAll();
                foreach (var (timer, remaining) in _timers.GetRunning())
                {
                    if (remaining > 0)
                        _notifications.Schedule("Таймер", $"«{timer.name}» завершён", remaining);
                }
            }
            else
            {
                // Вернулись: снимаем уведомления, в приложении сами показываем завершившиеся.
                _notifications.CancelAll();
                _timers.CompleteElapsed();
            }
        }
    }
}
