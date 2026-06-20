package com.mynative.androidnotify;

import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Build;
import android.os.PowerManager;
import android.provider.Settings;

/**
 * Точка входа из C# (через JNI). Планирует точный «будильник» через AlarmManager.
 * setAlarmClock — точный, переживает Doze и НЕ требует разрешений SCHEDULE_EXACT_ALARM
 * (трактуется системой как пользовательский будильник).
 */
public class AlarmApi {

    public static final String ACTION_FIRE = "com.mynative.androidnotify.FIRE";
    public static final String ACTION_STOP_TIMER = "com.mynative.androidnotify.STOP_TIMER";
    public static final String EXTRA_ID = "mnan_id";
    public static final String EXTRA_TITLE = "mnan_title";
    public static final String EXTRA_TEXT = "mnan_text";

    // «Идущее» уведомление таймера (постоянное, со «Стоп»). id уведомления = база + id таймера,
    // чтобы не пересекаться со звенящим FGS-уведомлением AlarmService.
    private static final String RUNNING_CHANNEL = "mnan_running";
    private static final int RUNNING_NOTIF_BASE = 0x52000000; // "R"

    // Хранилище id таймеров, остановленных кнопкой «Стоп» из уведомления (Unity доедает их при возврате).
    private static final String PREFS = "mnan_prefs";
    private static final String KEY_PENDING_STOP = "pending_stop"; // CSV id'шников
    // «Уже спрашивали» для разовых системных запросов надёжности фона (чтобы не открывать настройки каждый старт).
    private static final String KEY_ASKED_BATTERY = "asked_battery";
    private static final String KEY_ASKED_FSI = "asked_fsi";

    public static void schedule(Context ctx, int id, int secondsFromNow, String title, String text) {
        Context app = ctx.getApplicationContext();
        AlarmManager am = (AlarmManager) app.getSystemService(Context.ALARM_SERVICE);
        if (am == null) return;

        long triggerAt = System.currentTimeMillis() + (long) secondsFromNow * 1000L;
        PendingIntent fire = firePendingIntent(app, id, title, text);

        try {
            // showIntent ведёт на тот же fire; для setAlarmClock он обязателен
            AlarmManager.AlarmClockInfo info = new AlarmManager.AlarmClockInfo(triggerAt, fire);
            am.setAlarmClock(info, fire);
        } catch (Throwable t) {
            // запас на всякий случай
            am.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAt, fire);
        }
    }

    public static void cancel(Context ctx, int id) {
        Context app = ctx.getApplicationContext();
        AlarmManager am = (AlarmManager) app.getSystemService(Context.ALARM_SERVICE);
        if (am != null) am.cancel(firePendingIntent(app, id, "", ""));
    }

    /** Остановить звенящий сейчас будильник (если он играет). */
    public static void stopRinging(Context ctx) {
        Context app = ctx.getApplicationContext();
        app.stopService(new Intent(app, AlarmService.class));
    }

    // ── «Идущее» уведомление таймера (постоянное, со «Стоп») ─────────────────────────────

    /** Показать постоянное уведомление «таймер идёт» (заранее посчитанное время окончания + «Стоп»). */
    public static void showRunning(Context ctx, int id, String name, String endText) {
        Context app = ctx.getApplicationContext();
        NotificationManager nm = (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (nm == null) return;
        createRunningChannel(nm);

        int piFlags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) piFlags |= PendingIntent.FLAG_IMMUTABLE;

        // Тап по уведомлению — открыть приложение.
        PendingIntent contentPi = null;
        Intent launch = app.getPackageManager().getLaunchIntentForPackage(app.getPackageName());
        if (launch != null) {
            launch.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
            contentPi = PendingIntent.getActivity(app, RUNNING_NOTIF_BASE + id, launch, piFlags);
        }

        // «Стоп» — broadcast в AlarmReceiver (снимет будильник, уберёт уведомление, пометит остановку).
        Intent stopIntent = new Intent(app, AlarmReceiver.class);
        stopIntent.setAction(ACTION_STOP_TIMER);
        stopIntent.putExtra(EXTRA_ID, id);
        PendingIntent stopPi = PendingIntent.getBroadcast(app, RUNNING_NOTIF_BASE + id, stopIntent, piFlags);

        String safeName = (name == null || name.isEmpty()) ? "Таймер" : name;
        Notification.Builder b;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            b = new Notification.Builder(app, RUNNING_CHANNEL);
        } else {
            b = new Notification.Builder(app);
            b.setPriority(Notification.PRIORITY_LOW);
        }
        b.setContentTitle("Таймер «" + safeName + "»")
                .setContentText(endText == null || endText.isEmpty() ? "Идёт отсчёт" : "Сработает в " + endText)
                .setSmallIcon(android.R.drawable.ic_lock_idle_alarm)
                .setOngoing(true)
                .setOnlyAlertOnce(true)
                .addAction(android.R.drawable.ic_menu_close_clear_cancel, "Стоп", stopPi);
        if (contentPi != null) b.setContentIntent(contentPi);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
            b.setVisibility(Notification.VISIBILITY_PUBLIC);

        nm.notify(RUNNING_NOTIF_BASE + id, b.build());
    }

    /** Убрать «идущее» уведомление таймера. */
    public static void hideRunning(Context ctx, int id) {
        Context app = ctx.getApplicationContext();
        NotificationManager nm = (NotificationManager) app.getSystemService(Context.NOTIFICATION_SERVICE);
        if (nm != null) nm.cancel(RUNNING_NOTIF_BASE + id);
    }

    private static void createRunningChannel(NotificationManager nm) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return;
        NotificationChannel ch = new NotificationChannel(RUNNING_CHANNEL, "Идущие таймеры",
                NotificationManager.IMPORTANCE_LOW); // тихо, без звука и тряски
        ch.setDescription("Уведомление, что таймер запущен");
        ch.setSound(null, null);
        ch.enableVibration(false);
        ch.setShowBadge(false);
        nm.createNotificationChannel(ch);
    }

    // ── Пометки «остановлено из уведомления» (Unity доедает при возврате/запуске) ─────────

    /** Добавить id таймера в список остановленных кнопкой «Стоп» (зовётся из AlarmReceiver). */
    public static void addPendingStop(Context ctx, int id) {
        Context app = ctx.getApplicationContext();
        SharedPreferences sp = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        String csv = sp.getString(KEY_PENDING_STOP, "");
        csv = (csv == null || csv.isEmpty()) ? String.valueOf(id) : csv + "," + id;
        sp.edit().putString(KEY_PENDING_STOP, csv).commit();
    }

    /** Вернуть CSV остановленных id и очистить список (зовётся из C#). */
    public static String consumePendingStops(Context ctx) {
        Context app = ctx.getApplicationContext();
        SharedPreferences sp = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        String csv = sp.getString(KEY_PENDING_STOP, "");
        if (csv != null && !csv.isEmpty())
            sp.edit().remove(KEY_PENDING_STOP).commit();
        return csv == null ? "" : csv;
    }

    // ── Надёжность фона: батарейная оптимизация + full-screen-intent (Android 14+) ─────────

    /**
     * Разово (один раз на установку для каждого пункта) запросить у пользователя то, без чего
     * агрессивная прошивка глушит фоновый будильник: исключение из батарейной оптимизации
     * и разрешение full-screen-intent (Android 14+, у сайдлоад-приложений по умолчанию снято).
     * Открывает системный диалог/экран только если разрешение ещё НЕ выдано и его ещё не просили.
     */
    public static void requestBackgroundReliability(Context ctx) {
        Context app = ctx.getApplicationContext();

        if (!wasAsked(app, KEY_ASKED_BATTERY) && !isIgnoringBatteryOptimizations(app)) {
            markAsked(app, KEY_ASKED_BATTERY);
            try {
                Intent i = new Intent(Settings.ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS,
                        Uri.parse("package:" + app.getPackageName()));
                i.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                ctx.startActivity(i);
            } catch (Throwable ignored) {
            }
        }

        if (Build.VERSION.SDK_INT >= 34 && !wasAsked(app, KEY_ASKED_FSI) && !canUseFullScreenIntent(app)) {
            markAsked(app, KEY_ASKED_FSI);
            try {
                Intent i = new Intent(Settings.ACTION_MANAGE_APP_USE_FULL_SCREEN_INTENT,
                        Uri.parse("package:" + app.getPackageName()));
                i.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                ctx.startActivity(i);
            } catch (Throwable ignored) {
            }
        }
    }

    private static boolean isIgnoringBatteryOptimizations(Context ctx) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) return true;
        PowerManager pm = (PowerManager) ctx.getSystemService(Context.POWER_SERVICE);
        return pm == null || pm.isIgnoringBatteryOptimizations(ctx.getPackageName());
    }

    private static boolean canUseFullScreenIntent(Context ctx) {
        if (Build.VERSION.SDK_INT < 34) return true; // до Android 14 ограничения нет
        NotificationManager nm = (NotificationManager) ctx.getSystemService(Context.NOTIFICATION_SERVICE);
        return nm == null || nm.canUseFullScreenIntent();
    }

    private static boolean wasAsked(Context app, String key) {
        return app.getSharedPreferences(PREFS, Context.MODE_PRIVATE).getBoolean(key, false);
    }

    private static void markAsked(Context app, String key) {
        app.getSharedPreferences(PREFS, Context.MODE_PRIVATE).edit().putBoolean(key, true).commit();
    }

    private static PendingIntent firePendingIntent(Context app, int id, String title, String text) {
        Intent intent = new Intent(app, AlarmReceiver.class);
        intent.setAction(ACTION_FIRE);
        intent.putExtra(EXTRA_ID, id);
        intent.putExtra(EXTRA_TITLE, title);
        intent.putExtra(EXTRA_TEXT, text);

        int flags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
            flags |= PendingIntent.FLAG_IMMUTABLE;
        // requestCode = id → разные таймеры не затирают друг друга
        return PendingIntent.getBroadcast(app, id, intent, flags);
    }
}
