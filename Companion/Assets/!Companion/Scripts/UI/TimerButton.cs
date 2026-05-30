using System;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Элемент списка таймеров: название, тап — запуск (через SwipeRow), свайп влево
    /// открывает кнопку «Удалить». Создаётся клонированием шаблона, Zenject-инъекции не требует.
    /// </summary>
    public class TimerButton : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private Button deleteButton;
        [SerializeField] private SwipeRow swipeRow;

        public void Setup(TimerData timer, Action onStart, Action onDelete)
        {
            label.text = $"{timer.name} ({timer.minutes} мин)";

            swipeRow.TapWhenClosed = onStart;

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDelete?.Invoke());
        }
    }
}
