using System;
using System.Collections;
using System.Collections.Generic;
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

        private readonly CoroutineManager _coroutineManager;
        private readonly Dictionary<int, Running> _running = new Dictionary<int, Running>();

        public TimerService(CoroutineManager coroutineManager)
        {
            _coroutineManager = coroutineManager;
        }

        public bool IsRunning(int timerId) => _running.ContainsKey(timerId);

        /// <summary>Запустить таймер. Повторный запуск уже идущего игнорируется.</summary>
        public void StartTimer(TimerData timer)
        {
            if (timer == null || _running.ContainsKey(timer.id))
                return;

            var r = new Running { timer = timer, endUtc = DateTime.UtcNow.AddSeconds(timer.minutes * 60) };
            r.key = _coroutineManager.CoroutineParallel(RunTimer(r));
            _running[timer.id] = r;

            EventManager.OnActionSend(EventManager.TimerStarted, timer, timer.minutes * 60);
        }

        /// <summary>Досрочно остановить таймер (без сигнала завершения).</summary>
        public void StopTimer(int timerId)
        {
            if (_running.TryGetValue(timerId, out var r))
            {
                _coroutineManager.StopCoroutineByKey(r.key);
                _running.Remove(timerId);
                EventManager.OnActionSend(EventManager.TimerStopped, timerId, null);
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

        /// <summary>Завершить таймеры, чьё время уже истекло (например, пока приложение было в фоне).</summary>
        public void CompleteElapsed()
        {
            foreach (var r in new List<Running>(_running.Values)) // копия — Complete меняет словарь
            {
                if (RemainingSeconds(r) <= 0)
                    Complete(r);
            }
        }

        private static int RemainingSeconds(Running r)
        {
            return Mathf.Max(0, (int)Math.Ceiling((r.endUtc - DateTime.UtcNow).TotalSeconds));
        }

        private void Complete(Running r)
        {
            if (!_running.ContainsKey(r.timer.id))
                return;

            _running.Remove(r.timer.id);
            EventManager.OnActionSend(EventManager.TimerTick, r.timer.id, 0);
            EventManager.OnActionSend(EventManager.TimerCompleted, r.timer, null);
            _coroutineManager.StopCoroutineByKey(r.key);
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

            Complete(r);
        }
    }
}
