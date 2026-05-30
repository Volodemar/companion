package com.mynative.androidnotify;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ServiceInfo;
import android.media.AudioAttributes;
import android.media.MediaPlayer;
import android.media.RingtoneManager;
import android.net.Uri;
import android.os.Build;
import android.os.IBinder;
import android.os.PowerManager;

/**
 * Foreground-сервис: проигрывает зацикленный alarm-рингтон (на alarm-стриме —
 * слышно даже в тихом режиме) и держит ongoing-уведомление с кнопкой «Стоп» и
 * full-screen-интентом. Звенит, пока не остановят (broadcast ACTION_STOP → stopService).
 *
 * START_NOT_STICKY + игнор перезапуска с пустым интентом — чтобы система НЕ перезапускала
 * звон сама (например после свайпа приложения из недавних).
 */
public class AlarmService extends Service {

    public static final String ACTION_STOP = "com.mynative.androidnotify.STOP";

    private static final String CHANNEL_ID = "mnan_alarm";
    private static final int FGS_NOTIFICATION_ID = 0x4D4E41; // "MNA"

    private MediaPlayer player;
    private PowerManager.WakeLock wakeLock;

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        // Пустой интент = системный перезапуск сервиса: НЕ звеним заново, просто гаснем.
        if (intent == null || ACTION_STOP.equals(intent.getAction())) {
            stopSelf();
            return START_NOT_STICKY;
        }

        String title = intent.getStringExtra(AlarmApi.EXTRA_TITLE);
        String text = intent.getStringExtra(AlarmApi.EXTRA_TEXT);
        if (title == null || title.isEmpty()) title = "Таймер";
        if (text == null || text.isEmpty()) text = "Время вышло";

        createChannel();
        Notification notification = buildNotification(title, text);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            startForeground(FGS_NOTIFICATION_ID, notification, ServiceInfo.FOREGROUND_SERVICE_TYPE_MEDIA_PLAYBACK);
        } else {
            startForeground(FGS_NOTIFICATION_ID, notification);
        }

        acquireWakeLock();
        startRinging();
        return START_NOT_STICKY;
    }

    private void createChannel() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return;
        NotificationManager nm = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
        if (nm == null) return;
        NotificationChannel ch = new NotificationChannel(CHANNEL_ID, "Будильник таймера",
                NotificationManager.IMPORTANCE_HIGH);
        ch.setDescription("Сигнал о завершении таймера");
        // Звук канала отключаем — звеним сами через MediaPlayer (управляемое зацикливание).
        ch.setSound(null, null);
        ch.enableVibration(false);
        ch.setBypassDnd(true);
        nm.createNotificationChannel(ch);
    }

    @SuppressWarnings("deprecation")
    private Notification buildNotification(String title, String text) {
        int piFlags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) piFlags |= PendingIntent.FLAG_IMMUTABLE;

        // Full-screen экран будильника (отдельный таск, без CLEAR_TASK — чтобы не трогать Unity).
        Intent fullIntent = new Intent(this, AlarmActivity.class);
        fullIntent.putExtra(AlarmApi.EXTRA_TITLE, title);
        fullIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        PendingIntent fullPi = PendingIntent.getActivity(this, 1, fullIntent, piFlags);

        // «Стоп» — broadcast (надёжнее, чем startService из фона) → AlarmReceiver → stopService.
        Intent stopIntent = new Intent(this, AlarmReceiver.class);
        stopIntent.setAction(ACTION_STOP);
        PendingIntent stopPi = PendingIntent.getBroadcast(this, 3, stopIntent, piFlags);

        Notification.Builder b;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            b = new Notification.Builder(this, CHANNEL_ID);
        } else {
            b = new Notification.Builder(this);
            b.setPriority(Notification.PRIORITY_MAX);
        }
        b.setContentTitle(title)
                .setContentText(text)
                .setSmallIcon(android.R.drawable.ic_lock_idle_alarm)
                .setOngoing(true)
                .setAutoCancel(false)
                .setContentIntent(fullPi)
                .setFullScreenIntent(fullPi, true)
                .addAction(android.R.drawable.ic_menu_close_clear_cancel, "Стоп", stopPi);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            b.setCategory(Notification.CATEGORY_ALARM);
            b.setVisibility(Notification.VISIBILITY_PUBLIC);
        }
        return b.build();
    }

    private void startRinging() {
        try {
            Uri uri = RingtoneManager.getActualDefaultRingtoneUri(this, RingtoneManager.TYPE_ALARM);
            if (uri == null) uri = RingtoneManager.getDefaultUri(RingtoneManager.TYPE_ALARM);
            if (uri == null) uri = RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION);

            player = new MediaPlayer();
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                player.setAudioAttributes(new AudioAttributes.Builder()
                        .setUsage(AudioAttributes.USAGE_ALARM)
                        .setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION)
                        .build());
            } else {
                player.setAudioStreamType(android.media.AudioManager.STREAM_ALARM);
            }
            player.setDataSource(this, uri);
            player.setLooping(true);
            player.prepare();
            player.start();
        } catch (Throwable t) {
            // звук не критичен для падения сервиса — просто молчим
        }
    }

    private void acquireWakeLock() {
        try {
            PowerManager pm = (PowerManager) getSystemService(POWER_SERVICE);
            if (pm == null) return;
            wakeLock = pm.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "mnan:alarm");
            wakeLock.acquire(5 * 60 * 1000L); // максимум 5 минут звона
        } catch (Throwable ignored) {
        }
    }

    private void stopRinging() {
        if (player != null) {
            try { player.stop(); } catch (Throwable ignored) {}
            try { player.release(); } catch (Throwable ignored) {}
            player = null;
        }
    }

    @Override
    public void onDestroy() {
        stopRinging();
        if (wakeLock != null && wakeLock.isHeld()) {
            try { wakeLock.release(); } catch (Throwable ignored) {}
        }
        wakeLock = null;
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N)
                stopForeground(Service.STOP_FOREGROUND_REMOVE);
            else
                stopForeground(true);
        } catch (Throwable ignored) {}
        super.onDestroy();
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}
