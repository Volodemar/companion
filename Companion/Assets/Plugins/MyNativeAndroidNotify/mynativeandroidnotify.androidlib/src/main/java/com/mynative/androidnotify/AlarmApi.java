package com.mynative.androidnotify;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.Build;

/**
 * Точка входа из C# (через JNI). Планирует точный «будильник» через AlarmManager.
 * setAlarmClock — точный, переживает Doze и НЕ требует разрешений SCHEDULE_EXACT_ALARM
 * (трактуется системой как пользовательский будильник).
 */
public class AlarmApi {

    public static final String ACTION_FIRE = "com.mynative.androidnotify.FIRE";
    public static final String EXTRA_ID = "mnan_id";
    public static final String EXTRA_TITLE = "mnan_title";
    public static final String EXTRA_TEXT = "mnan_text";

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
