using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace MyNativeAndroidNotify
{
    /// <summary>
    /// Переносимая обёртка над нативным Android-будильником (плагин MyNativeAndroidNotify).
    /// Планирует системный «будильник», который звенит зацикленным alarm-рингтоном,
    /// показывает полноэкранный экран со «Стоп» и ongoing-уведомление — даже если
    /// приложение свёрнуто или закрыто. В редакторе и не на Android — методы-пустышки.
    ///
    /// API не зависит от конкретного проекта: id — произвольный идентификатор «будильника».
    /// </summary>
    public static class AlarmNotify
    {
        private const string JavaClass = "com.mynative.androidnotify.AlarmApi";

        /// <summary>Запросить разрешение на уведомления (Android 13+). Звать при старте.</summary>
        public static void RequestNotificationPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            const string perm = "android.permission.POST_NOTIFICATIONS";
            if (!Permission.HasUserAuthorizedPermission(perm))
                Permission.RequestUserPermission(perm);
#endif
        }

        /// <summary>Запланировать будильник через secondsFromNow секунд.</summary>
        public static void Schedule(int id, int secondsFromNow, string title, string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var api = new AndroidJavaClass(JavaClass))
            using (var ctx = CurrentActivity())
                api.CallStatic("schedule", ctx, id, secondsFromNow, title ?? string.Empty, text ?? string.Empty);
#endif
        }

        /// <summary>Отменить запланированный будильник по id.</summary>
        public static void Cancel(int id)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var api = new AndroidJavaClass(JavaClass))
            using (var ctx = CurrentActivity())
                api.CallStatic("cancel", ctx, id);
#endif
        }

        /// <summary>Остановить звенящий сейчас будильник.</summary>
        public static void StopRinging()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var api = new AndroidJavaClass(JavaClass))
            using (var ctx = CurrentActivity())
                api.CallStatic("stopRinging", ctx);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject CurrentActivity()
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                return player.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
    }
}
