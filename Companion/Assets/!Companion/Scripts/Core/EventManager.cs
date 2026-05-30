using System;

namespace Companion.Core
{
    /// <summary>
    /// Глобальная шина событий: развязывает отправителей и получателей.
    /// Подписка: EventManager.OnAction += Handler; внутри обработчика фильтр по ID.
    /// Портировано из TheFirst, оставлены только события таймеров.
    /// </summary>
    public static class EventManager
    {
        // Таймеры
        public static int TimerStarted   => 10;  // obj: TimerData, obj2: int секунд
        public static int TimerTick      => 11;  // obj: int id,   obj2: int остаток секунд
        public static int TimerCompleted => 12;  // obj: TimerData, obj2: bool ring (играть ли in-app сигнал)
        public static int TimerStopped   => 13;  // obj: int id,    obj2: null

        public static Action<int, object, object> OnAction;

        public static void OnActionSend(int id, object obj, object obj2)
        {
            OnAction?.Invoke(id, obj, obj2);
        }
    }
}
