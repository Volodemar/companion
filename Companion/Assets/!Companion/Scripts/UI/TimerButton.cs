using System;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Элемент списка таймеров: кнопка с названием, по нажатию запускает таймер.
    /// Создаётся клонированием шаблона, Zenject-инъекции не требует.
    /// </summary>
    public class TimerButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text label;

        public void Setup(TimerData timer, Action onClick)
        {
            label.text = $"{timer.name} ({timer.minutes} мин)";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
