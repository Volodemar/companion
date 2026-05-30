using System.Collections.Generic;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Companion.UI
{
    /// <summary>
    /// Попап завершения таймера. Подписку на событие держит
    /// <see cref="TimerPopupController"/>, поэтому попап может лежать в сцене выключенным.
    /// Пока попап не подтверждён (кнопка ОК), звучит будильник; при нескольких
    /// завершившихся таймерах они показываются по очереди, а звон не прерывается
    /// до подтверждения последнего.
    /// </summary>
    public class UIPopupTimerDone : UIPopup
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Button buttonOk;

        [Inject] private AudioManager _audio;

        private readonly Queue<string> _queue = new Queue<string>();

        private void Awake()
        {
            buttonOk.onClick.AddListener(OnOkClicked);
        }

        /// <summary>Добавить завершившийся таймер в очередь и включить будильник.</summary>
        public void ShowDone(string name)
        {
            _queue.Enqueue(name);
            _audio.StartAlarm();

            if (!gameObject.activeSelf)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
                return;

            string name = _queue.Dequeue();
            labelText.text = $"Таймер «{name}» завершён";
            Show();
        }

        private void OnOkClicked()
        {
            Hide();

            // Звон останавливаем только когда подтверждён последний таймер в очереди.
            if (_queue.Count == 0)
                _audio.StopAlarm();
            else
                ShowNext();
        }
    }
}
