package com.mynative.androidnotify;

import android.app.Activity;
import android.graphics.Color;
import android.os.Build;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;

/**
 * Полноэкранный экран будильника поверх блокировки (показывается через
 * full-screen-intent уведомления). Кнопка «Стоп» гасит сервис. UI строится кодом —
 * библиотека не тащит ресурсы, остаётся переносимой.
 */
public class AlarmActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O_MR1) {
            setShowWhenLocked(true);
            setTurnScreenOn(true);
        } else {
            getWindow().addFlags(
                    WindowManager.LayoutParams.FLAG_SHOW_WHEN_LOCKED
                            | WindowManager.LayoutParams.FLAG_TURN_SCREEN_ON
                            | WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        }

        String title = getIntent() != null ? getIntent().getStringExtra(AlarmApi.EXTRA_TITLE) : null;
        if (title == null || title.isEmpty()) title = "Таймер";

        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        root.setGravity(Gravity.CENTER);
        root.setBackgroundColor(Color.rgb(20, 20, 20));
        int pad = dp(32);
        root.setPadding(pad, pad, pad, pad);

        TextView label = new TextView(this);
        label.setText(title);
        label.setTextColor(Color.WHITE);
        label.setTextSize(28);
        label.setGravity(Gravity.CENTER);

        TextView sub = new TextView(this);
        sub.setText("Таймер завершён");
        sub.setTextColor(Color.rgb(180, 180, 180));
        sub.setTextSize(16);
        sub.setGravity(Gravity.CENTER);
        LinearLayout.LayoutParams subLp = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT);
        subLp.bottomMargin = dp(40);
        sub.setLayoutParams(subLp);

        Button stop = new Button(this);
        stop.setText("Стоп");
        stop.setTextSize(20);
        stop.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                AlarmApi.stopRinging(AlarmActivity.this);
                finish();
            }
        });

        root.addView(label);
        root.addView(sub);
        root.addView(stop);
        setContentView(root);
    }

    private int dp(int value) {
        float density = getResources().getDisplayMetrics().density;
        return Math.round(value * density);
    }
}
