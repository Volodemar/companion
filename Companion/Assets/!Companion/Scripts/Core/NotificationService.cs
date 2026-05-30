using System;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif

namespace Companion.Core
{
    /// <summary>
    /// Локальные уведомления (Android): планируют сигнал о завершении таймера через ОС,
    /// чтобы он сработал даже когда приложение свёрнуто или закрыто. В редакторе и на
    /// прочих платформах все методы — пустышки (уведомления проверяются только на устройстве).
    /// </summary>
    public class NotificationService
    {
        private const string ChannelId = "timers";

        public NotificationService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            RegisterChannel();
            // Android 13+: системный диалог разрешения (на старых версиях — сразу Allowed).
            new PermissionRequest();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void RegisterChannel()
        {
            var channel = new AndroidNotificationChannel
            {
                Id = ChannelId,
                Name = "Таймеры",
                Description = "Сигналы о завершении таймеров",
                Importance = Importance.High,
                EnableVibration = false,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }
#endif

        /// <summary>Запланировать уведомление через secondsFromNow секунд. Возвращает id (или -1).</summary>
        public int Schedule(string title, string text, int secondsFromNow)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var notification = new AndroidNotification
            {
                Title = title,
                Text = text,
                FireTime = DateTime.Now.AddSeconds(secondsFromNow),
            };
            return AndroidNotificationCenter.SendNotification(notification, ChannelId);
#else
            return -1;
#endif
        }

        /// <summary>Снять все запланированные и уже показанные уведомления.</summary>
        public void CancelAll()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
#endif
        }
    }
}
