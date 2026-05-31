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

        /// <summary>Показать постоянное «идущее» уведомление таймера (время окончания + «Стоп»).</summary>
        public static void ShowRunning(int id, string name, string endText)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var api = new AndroidJavaClass(JavaClass))
                using (var ctx = CurrentActivity())
                    api.CallStatic("showRunning", ctx, id, name ?? string.Empty, endText ?? string.Empty);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error: Ошибка показа уведомления таймера: {e.Message}");
            }
#endif
        }

        /// <summary>Убрать «идущее» уведомление таймера.</summary>
        public static void HideRunning(int id)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var api = new AndroidJavaClass(JavaClass))
                using (var ctx = CurrentActivity())
                    api.CallStatic("hideRunning", ctx, id);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error: Ошибка скрытия уведомления таймера: {e.Message}");
            }
#endif
        }

        /// <summary>
        /// Забрать id таймеров, остановленных кнопкой «Стоп» из уведомления (и очистить список).
        /// Пусто в редакторе/не на Android.
        /// </summary>
        public static int[] ConsumePendingStops()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                string csv;
                using (var api = new AndroidJavaClass(JavaClass))
                using (var ctx = CurrentActivity())
                    csv = api.CallStatic<string>("consumePendingStops", ctx);

                if (string.IsNullOrEmpty(csv))
                    return System.Array.Empty<int>();

                string[] parts = csv.Split(',');
                var list = new System.Collections.Generic.List<int>(parts.Length);
                foreach (var p in parts)
                    if (int.TryParse(p, out int v))
                        list.Add(v);
                return list.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error: Ошибка чтения остановленных таймеров: {e.Message}");
                return System.Array.Empty<int>();
            }
#else
            return System.Array.Empty<int>();
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
