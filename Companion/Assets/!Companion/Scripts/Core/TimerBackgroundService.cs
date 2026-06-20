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
            // Разово попросить исключение из батарейной оптимизации и full-screen-intent (Android 14+):
            // без них агрессивная прошивка (MIUI/HyperOS) замораживает процесс и придерживает будильник.
            AlarmNotify.RequestBackgroundReliability();
        }

        private void OnApplicationPause(bool paused)
        {
            // Уходим в фон — ничего планировать не нужно: будильник вооружён ещё при старте таймера
            // (TimerService.StartTimer), поэтому он сработает, даже если процесс свернут/заморозят.
            if (paused)
                return;

            // Вернулись: глушим возможный звон будильника. Будильники ещё идущих таймеров НЕ снимаем —
            // они должны оставаться вооружёнными на случай повторного сворачивания.
            AlarmNotify.StopRinging();

            // Доесть таймеры, остановленные кнопкой «Стоп» из уведомления, пока были в фоне
            // (native снял будильник/уведомление, здесь убираем индикатор и запись). ДО CompleteElapsed,
            // чтобы остановленный таймер не «завершился» попапом.
            _timers.ApplyPendingStops();

            // Истёкшие в фоне: показать попап (какой таймер сработал), но БЕЗ in-app звука
            // (нативный будильник уже отзвенел). CompleteElapsed → Complete снимет их будильники.
            _timers.CompleteElapsed();
        }
    }
}
