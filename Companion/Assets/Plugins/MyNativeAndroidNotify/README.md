# MyNativeAndroidNotify

Переносимый плагин: системный «будильник» на Android, который **звенит, пока его не выключишь**,
даже если приложение свёрнуто или закрыто. Звук — зацикленный alarm-рингтон на alarm-стриме
(слышно в тихом режиме), плюс полноэкранный экран со «Стоп» и постоянное уведомление.

## Перенос в другой проект

Скопировать всю папку `Assets/Plugins/MyNativeAndroidNotify/` в другой проект. Больше ничего
не требуется — Java-библиотека (`mynativeandroidnotify.androidlib`) и C#-обёртка самодостаточны.
Требуется **Unity 6+** (Android Gradle Plugin 8, namespace задаётся в `build.gradle`).

## Использование (C#)

```csharp
using MyNativeAndroidNotify;

// один раз при старте (Android 13+ спросит разрешение на уведомления)
AlarmNotify.RequestNotificationPermission();

// запланировать будильник на 600 секунд вперёд (id — любой ваш идентификатор)
AlarmNotify.Schedule(id: 1, secondsFromNow: 600, title: "Таймер", text: "Время вышло");

// отменить запланированный
AlarmNotify.Cancel(1);

// остановить звенящий сейчас сигнал
AlarmNotify.StopRinging();
```

Типичная схема для таймера: планировать при сворачивании приложения (`OnApplicationPause(true)`),
отменять и останавливать при возврате (`OnApplicationPause(false)`), чтобы в активном приложении
работал собственный звук, а в фоне — системный будильник.

## Что внутри

- `AlarmApi.java` — планирование через `AlarmManager.setAlarmClock` (точно, переживает Doze,
  не требует разрешения SCHEDULE_EXACT_ALARM).
- `AlarmReceiver.java` — срабатывает в назначенное время, запускает сервис.
- `AlarmService.java` — foreground-сервис: зацикленный alarm-рингтон + ongoing-уведомление
  с кнопкой «Стоп» + full-screen-intent.
- `AlarmActivity.java` — полноэкранный экран будильника поверх блокировки (UI кодом, без ресурсов).
- `AlarmNotify.cs` — C#-обёртка (JNI), на не-Android/в редакторе — пустышки.

## Разрешения (добавляются манифестом библиотеки автоматически)

`POST_NOTIFICATIONS`, `FOREGROUND_SERVICE`, `FOREGROUND_SERVICE_MEDIA_PLAYBACK`, `WAKE_LOCK`,
`USE_FULL_SCREEN_INTENT`. Разрешения на точные будильники не нужны (используется `setAlarmClock`).

## Ограничения / проверять на устройстве

- Звон ограничен 5 минутами (wake-lock) — потом сам отпускает.
- Перезапланирование после перезагрузки телефона не реализовано (нет `BOOT_COMPLETED`).
- На «агрессивных» прошивках (Xiaomi/Huawei и т.п.) фоновые сервисы могут душиться — может
  потребоваться исключение из оптимизации батареи.
- `USE_FULL_SCREEN_INTENT` на Android 14+ для не-будильниковых приложений ограничен; для
  таймера допустимо, иначе деградирует до heads-up уведомления.
