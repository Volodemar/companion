using System.Collections.Generic;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Companion.UI
{
    /// <summary>
    /// Окно раздела «Таймер»: кнопка «Добавить» и список ранее созданных таймеров.
    /// Тап по таймеру запускает его (отсчёт идёт в TimerService).
    /// </summary>
    public class UIWindowTimer : UIWindow
    {
        [SerializeField] private Button buttonAdd;
        [SerializeField] private Button buttonBack;
        [SerializeField] private RectTransform content;          // контейнер списка
        [SerializeField] private TimerButton timerButtonPrefab;  // шаблон элемента списка

        [Inject] private TimersStorage _storage;
        [Inject] private TimerService _timerService;

        private readonly List<TimerButton> _buttons = new List<TimerButton>();

        private void Awake()
        {
            buttonAdd.onClick.AddListener(OnAddClicked);
            if (buttonBack != null)
                buttonBack.onClick.AddListener(OnBackClicked);
        }

        protected override void OnShow()
        {
            base.OnShow();
            RebuildList();
        }

        private void RebuildList()
        {
            foreach (var button in _buttons)
                Destroy(button.gameObject);
            _buttons.Clear();

            foreach (var timer in _storage.Timers)
            {
                var button = Instantiate(timerButtonPrefab, content);
                button.gameObject.SetActive(true);

                var captured = timer; // замыкание на конкретный таймер
                button.Setup(
                    captured,
                    onStart: () => _timerService.StartTimer(captured),
                    onDelete: () =>
                    {
                        _timerService.StopTimer(captured.id); // если шёл — остановить
                        _storage.Remove(captured.id);
                        RebuildList();
                    });
                _buttons.Add(button);
            }
        }

        private void OnAddClicked()
        {
            _ui.GetPopup<UIPopupTimerCreate>().Open(onCreated: RebuildList);
        }

        private void OnBackClicked()
        {
            _ui.GetWindow<UIWindowMain>().Show();
        }
    }
}
