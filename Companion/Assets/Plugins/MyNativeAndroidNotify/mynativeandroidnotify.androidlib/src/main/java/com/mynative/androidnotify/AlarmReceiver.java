package com.mynative.androidnotify;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Build;

/**
 * Срабатывает в назначенное время (даже если приложение закрыто) → запускает сервис.
 * Также принимает ACTION_STOP (из кнопки «Стоп» уведомления) → останавливает сервис.
 */
public class AlarmReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context ctx, Intent intent) {
        Context app = ctx.getApplicationContext();
        String action = intent != null ? intent.getAction() : null;

        if (AlarmService.ACTION_STOP.equals(action)) {
            app.stopService(new Intent(app, AlarmService.class));
            return;
        }

        int id = intent != null ? intent.getIntExtra(AlarmApi.EXTRA_ID, 0) : 0;

        // «Стоп» из постоянного «идущего» уведомления: снять будильник, убрать уведомление,
        // пометить таймер остановленным (Unity доест пометку при возврате/запуске).
        if (AlarmApi.ACTION_STOP_TIMER.equals(action)) {
            AlarmApi.cancel(app, id);
            AlarmApi.hideRunning(app, id);
            AlarmApi.addPendingStop(app, id);
            return;
        }

        // Срабатывание: «идущее» уведомление больше не нужно — его заменит звенящее.
        AlarmApi.hideRunning(app, id);

        Intent svc = new Intent(app, AlarmService.class);
        svc.putExtra(AlarmApi.EXTRA_ID, id);
        svc.putExtra(AlarmApi.EXTRA_TITLE, intent != null ? intent.getStringExtra(AlarmApi.EXTRA_TITLE) : null);
        svc.putExtra(AlarmApi.EXTRA_TEXT, intent != null ? intent.getStringExtra(AlarmApi.EXTRA_TEXT) : null);

        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
                app.startForegroundService(svc);
            else
                app.startService(svc);
        } catch (Throwable t) {
            // не валим приёмник, если система не дала стартовать сервис
        }
    }
}
