using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Запуск и отсчёт таймеров. Несколько таймеров идут параллельно.
    /// О событиях сообщает через EventManager — UI на сам сервис не завязан.
    /// </summary>
    public class TimerService
    {
        private readonly CoroutineManager _coroutineManager;

        // id таймера -> ключ его корутины в CoroutineManager (нужен для остановки)
        private readonly Dictionary<int, string> _running = new Dictionary<int, string>();

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

            string key = _coroutineManager.CoroutineParallel(RunTimer(timer));
            _running[timer.id] = key;

            EventManager.OnActionSend(EventManager.TimerStarted, timer, timer.minutes * 60);
        }

        /// <summary>Досрочно остановить таймер (без попапа завершения).</summary>
        public void StopTimer(int timerId)
        {
            if (_running.TryGetValue(timerId, out string key))
            {
                _coroutineManager.StopCoroutineByKey(key);
                _running.Remove(timerId);
                EventManager.OnActionSend(EventManager.TimerStopped, timerId, null);
            }
        }

        private IEnumerator RunTimer(TimerData timer)
        {
            int remaining = timer.minutes * 60;

            while (remaining > 0)
            {
                EventManager.OnActionSend(EventManager.TimerTick, timer.id, remaining);
                yield return new WaitForSeconds(1f);
                remaining--;
            }

            EventManager.OnActionSend(EventManager.TimerTick, timer.id, 0);
            _running.Remove(timer.id);
            EventManager.OnActionSend(EventManager.TimerCompleted, timer, null);
        }
    }
}
