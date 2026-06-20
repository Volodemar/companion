using System;
using System.Collections;
using System.Collections.Generic;
using MyNativeAndroidNotify;
using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Запуск и отсчёт таймеров. Несколько таймеров идут параллельно.
    /// Отсчёт ведётся по абсолютному времени (DateTime), а не по кадрам — поэтому
    /// корректно переживает паузу приложения в фоне. О событиях сообщает через
    /// EventManager — UI на сам сервис не завязан.
    /// </summary>
    public class TimerService
    {
        private class Running
        {
            public TimerData timer;
            public DateTime endUtc;
            public string key; // ключ корутины в CoroutineManager (для остановки)
        }

        // Буфер к будильнику: ставим его на (длительность + буфер), чтобы на переднем плане in-app
        // сигнал успел сработать в точное время и снять будильник (без двойного звона), а в фоне,
        // где Unity заморожен и снять некому, будильник звенит сам — с небольшой задержкой.
        private const int AlarmEndBufferSeconds = 3;

        private readonly CoroutineManager _coroutineManager;
        private readonly TimersStorage _storage;
        private readonly TimerRunsStorage _runs;
        private readonly Dictionary<int, Running> _running = new Dictionary<int, Running>();

        public TimerService(CoroutineManager coroutineManager, TimersStorage storage, TimerRunsStorage runs)
        {
            _coroutineManager = coroutineManager;
            _storage = storage;
            _runs = runs;

            // Восстановление идущих таймеров после перезапуска приложения — через кадр,
            // чтобы UI и подписчики шины (индикаторы, попап) успели подняться.
            _coroutineManager.CoroutineParallel(RestoreRunningTimers());
        }

        public bool IsRunning(int timerId) => _running.ContainsKey(timerId);

        /// <summary>Запустить таймер. Повторный запуск уже идущего игнорируется.</summary>
        public void StartTimer(TimerData timer)
        {
            if (timer == null || _running.ContainsKey(timer.id))
                return;

            int durationSeconds = timer.minutes * 60;
            var r = new Running { timer = timer, endUtc = DateTime.UtcNow.AddSeconds(durationSeconds) };
            r.key = _coroutineManager.CoroutineParallel(RunTimer(r));
            _running[timer.id] = r;
            _runs.StartRun(timer.id, r.endUtc); // запоминаем идущий таймер на диск (переживает перезапуск)

            EventManager.OnActionSend(EventManager.TimerStarted, timer, durationSeconds);

            // Системный будильник планируем СРАЗУ при старте (а не при сворачивании) — так он
            // надёжно вооружён к моменту, когда приложение свернут/заморозят. Зовётся ПОСЛЕ события,
            // чтобы любой сбой плагина не помешал запуску таймера и обновлению UI.
            // title = имя таймера: его покажет и full-screen-экран будильника, и уведомление.
            AlarmNotify.Schedule(timer.id, durationSeconds + AlarmEndBufferSeconds,
                timer.name, "Время вышло");

            // Постоянное «идущее» уведомление: заранее посчитанное время окончания (не тикает) + «Стоп».
            AlarmNotify.ShowRunning(timer.id, timer.name, DateTime.Now.AddSeconds(durationSeconds).ToString("HH:mm"));
        }

        /// <summary>Досрочно остановить таймер (без сигнала завершения).</summary>
        public void StopTimer(int timerId)
        {
            if (_running.TryGetValue(timerId, out var r))
            {
                _coroutineManager.StopCoroutineByKey(r.key);
                _running.Remove(timerId);
                _runs.EndRun(timerId); // убрать из сохранённых идущих
                EventManager.OnActionSend(EventManager.TimerStopped, timerId, null);
                AlarmNotify.Cancel(timerId);      // снять запланированный будильник
                AlarmNotify.HideRunning(timerId); // убрать «идущее» уведомление
            }
        }

        /// <summary>Идущие таймеры и остаток секунд каждого (для планирования уведомлений в фоне).</summary>
        public List<(TimerData timer, int remaining)> GetRunning()
        {
            var list = new List<(TimerData, int)>();
            foreach (var r in _running.Values)
                list.Add((r.timer, RemainingSeconds(r)));
            return list;
        }

        /// <summary>
        /// Завершить таймеры, истёкшие в фоне (вызывается на возврате в приложение).
        /// ring=false: попап «какой таймер сработал» покажем, но in-app сигнал НЕ играем —
        /// нативный будильник уже отзвенел в фоне.
        /// </summary>
        public void CompleteElapsed()
        {
            foreach (var r in new List<Running>(_running.Values)) // копия — Complete меняет словарь
            {
                if (RemainingSeconds(r) <= 0)
                    Complete(r, ring: false);
            }
        }

        private static int RemainingSeconds(Running r)
        {
            return Mathf.Max(0, (int)Math.Ceiling((r.endUtc - DateTime.UtcNow).TotalSeconds));
        }

        private void Complete(Running r, bool ring)
        {
            if (!_running.ContainsKey(r.timer.id))
                return;

            _running.Remove(r.timer.id);
            _runs.EndRun(r.timer.id); // убрать из сохранённых идущих
            EventManager.OnActionSend(EventManager.TimerTick, r.timer.id, 0);
            EventManager.OnActionSend(EventManager.TimerCompleted, r.timer, ring);
            _coroutineManager.StopCoroutineByKey(r.key);

            // Снимаем будильник: на переднем плане (ring=true) — чтобы он не зазвенел вдобавок к
            // in-app сигналу; в фоне (ring=false) он уже отзвенел, отмена просто безвредна.
            AlarmNotify.Cancel(r.timer.id);
            AlarmNotify.HideRunning(r.timer.id); // «идущее» уведомление больше не нужно
        }

        private IEnumerator RunTimer(Running r)
        {
            while (true)
            {
                int remaining = RemainingSeconds(r);
                if (remaining <= 0)
                    break;

                EventManager.OnActionSend(EventManager.TimerTick, r.timer.id, remaining);
                yield return new WaitForSeconds(1f);
            }

            // На случай, если этот таймер успели остановить кнопкой «Стоп» из уведомления, пока
            // приложение было на переднем плане: доедаем пометку — тогда Complete ниже не зазвенит.
            ApplyPendingStops();
            Complete(r, ring: true);
        }

        /// <summary>
        /// Доесть таймеры, остановленные кнопкой «Стоп» из уведомления (native снял будильник и
        /// уведомление сразу, но состояние/индикатор в приложении надо привести в порядок здесь).
        /// Зовётся при возврате в приложение, при запуске и перед завершением таймера.
        /// </summary>
        public void ApplyPendingStops()
        {
            int[] ids = AlarmNotify.ConsumePendingStops();
            if (ids == null || ids.Length == 0)
                return;

            foreach (int id in ids)
            {
                if (_running.TryGetValue(id, out var r))
                {
                    _coroutineManager.StopCoroutineByKey(r.key);
                    _running.Remove(id);
                    EventManager.OnActionSend(EventManager.TimerStopped, id, null); // убрать индикатор
                }
                _runs.EndRun(id);
                AlarmNotify.Cancel(id);
                AlarmNotify.HideRunning(id);
            }
        }

        /// <summary>
        /// Восстановить идущие таймеры после перезапуска приложения (отсчёт хранится только в памяти,
        /// но момент завершения сохранён в TimerRunsStorage). Ждём кадр, чтобы UI и подписчики шины
        /// поднялись. Ещё идущие — снова запускаем (индикатор + перевзведённый будильник); истёкшие,
        /// пока приложение было закрыто, — показываем попапом «сработал» БЕЗ in-app звука.
        /// </summary>
        private IEnumerator RestoreRunningTimers()
        {
            yield return null; // дать AppController/индикаторам/попапу инициализироваться

            AlarmNotify.StopRinging(); // заглушить возможный залипший звон до показа попапов
            ApplyPendingStops();       // доесть таймеры, остановленные кнопкой «Стоп» в уведомлении

            foreach (var run in new List<TimerRunsStorage.Run>(_runs.Runs)) // копия — список изменится
            {
                // Найти определение таймера; если его удалили — запись осиротевшая, чистим.
                TimerData timer = null;
                foreach (var t in _storage.Timers)
                {
                    if (t.id == run.timerId) { timer = t; break; }
                }

                DateTime endUtc;
                bool parsed = DateTime.TryParse(run.endUtcIso, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind, out endUtc);

                if (timer == null || !parsed || _running.ContainsKey(run.timerId))
                {
                    _runs.EndRun(run.timerId);
                    AlarmNotify.Cancel(run.timerId);
                    continue;
                }

                var r = new Running { timer = timer, endUtc = endUtc.ToUniversalTime() };
                int remaining = RemainingSeconds(r);

                if (remaining > 0)
                {
                    // Ещё идёт: восстановить отсчёт + перевзвести будильник (после ребута он стёрт).
                    r.key = _coroutineManager.CoroutineParallel(RunTimer(r));
                    _running[timer.id] = r;
                    AlarmNotify.Schedule(timer.id, remaining + AlarmEndBufferSeconds,
                        timer.name, "Время вышло");
                    AlarmNotify.ShowRunning(timer.id, timer.name, r.endUtc.ToLocalTime().ToString("HH:mm"));
                    EventManager.OnActionSend(EventManager.TimerStarted, timer, remaining);
                }
                else
                {
                    // Истёк, пока приложение было закрыто: попап без in-app звука, снять будильник/запись.
                    EventManager.OnActionSend(EventManager.TimerCompleted, timer, false);
                    AlarmNotify.Cancel(timer.id);
                    AlarmNotify.HideRunning(timer.id);
                    _runs.EndRun(timer.id);
                }
            }
        }
    }
}
