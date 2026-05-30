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

        Intent svc = new Intent(app, AlarmService.class);
        svc.putExtra(AlarmApi.EXTRA_ID, intent != null ? intent.getIntExtra(AlarmApi.EXTRA_ID, 0) : 0);
        svc.putExtra(AlarmApi.EXTRA_TITLE, intent != null ? intent.getStringExtra(AlarmApi.EXTRA_TITLE) : null);
        svc.putExtra(AlarmApi.EXTRA_TEXT, intent != null ? intent.getStringExtra(AlarmApi.EXTRA_TEXT) : null);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
            app.startForegroundService(svc);
        else
            app.startService(svc);
    }
}
