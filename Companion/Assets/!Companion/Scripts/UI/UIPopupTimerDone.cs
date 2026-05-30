using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Попап завершения таймера. Подписку на событие держит не он сам, а
    /// <see cref="TimerPopupController"/> — поэтому попап может лежать в сцене
    /// выключенным. Если завершилось несколько таймеров — показываются по очереди.
    /// </summary>
    public class UIPopupTimerDone : UIPopup
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Button buttonOk;

        private readonly Queue<string> _queue = new Queue<string>();

        private void Awake()
        {
            buttonOk.onClick.AddListener(OnOkClicked);
        }

        /// <summary>Поставить таймер в очередь показа; если попап свободен — показать сразу.</summary>
        public void ShowDone(string name)
        {
            _queue.Enqueue(name);

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
            ShowNext();
        }
    }
}
