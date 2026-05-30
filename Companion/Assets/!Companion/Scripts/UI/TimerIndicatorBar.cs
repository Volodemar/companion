using System.Collections.Generic;
using Companion.Core;
using UnityEngine;

namespace Companion.UI
{
    /// <summary>
    /// Полоса индикаторов внизу экрана: по кружку на каждый идущий таймер.
    /// Живёт вне окон (прямо под Canvas), поэтому виден и после возврата из
    /// окна «Таймер» в главное окно. Завязки на таймеры нет — только EventManager.
    /// </summary>
    public class TimerIndicatorBar : MonoBehaviour
    {
        [SerializeField] private TimerIndicator indicatorPrefab; // шаблон кружка
        [SerializeField] private Transform container;

        private readonly Dictionary<int, TimerIndicator> _items = new Dictionary<int, TimerIndicator>();

        private void Awake()
        {
            EventManager.OnAction += OnEvent;
        }

        private void OnDestroy()
        {
            EventManager.OnAction -= OnEvent;
        }

        private void OnEvent(int id, object obj, object obj2)
        {
            if (id == EventManager.TimerStarted)
            {
                if (obj is TimerData timer)
                    AddIndicator(timer);
            }
            else if (id == EventManager.TimerTick)
            {
                int timerId = (int)obj;
                int remaining = (int)obj2;
                if (_items.TryGetValue(timerId, out var item))
                    item.SetRemaining(remaining);
            }
            else if (id == EventManager.TimerCompleted)
            {
                if (obj is TimerData timer)
                    RemoveIndicator(timer.id);
            }
            else if (id == EventManager.TimerStopped)
            {
                RemoveIndicator((int)obj);
            }
        }

        private void AddIndicator(TimerData timer)
        {
            if (_items.ContainsKey(timer.id))
                return;

            var item = Instantiate(indicatorPrefab, container);
            item.gameObject.SetActive(true);
            item.Setup(timer);
            _items[timer.id] = item;
        }

        private void RemoveIndicator(int timerId)
        {
            if (_items.TryGetValue(timerId, out var item))
            {
                Destroy(item.gameObject);
                _items.Remove(timerId);
            }
        }
    }
}
