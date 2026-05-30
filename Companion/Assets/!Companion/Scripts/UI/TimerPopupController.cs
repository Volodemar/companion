using System;
using Companion.Core;
using Zenject;

namespace Companion.UI
{
    /// <summary>
    /// Показывает попап завершения таймера независимо от текущего окна.
    /// Слушает шину здесь, а не в самом попапе, чтобы попап мог лежать в сцене
    /// выключенным (у выключенного объекта Awake не выполняется и подписки бы не было).
    /// </summary>
    public class TimerPopupController : IInitializable, IDisposable
    {
        private readonly UIManager _ui;

        public TimerPopupController(UIManager ui)
        {
            _ui = ui;
        }

        public void Initialize()
        {
            EventManager.OnAction += OnEvent;
        }

        public void Dispose()
        {
            EventManager.OnAction -= OnEvent;
        }

        private void OnEvent(int id, object obj, object obj2)
        {
            if (id != EventManager.TimerCompleted)
                return;

            string name = (obj as TimerData)?.name ?? "Таймер";
            bool ring = obj2 is bool b && b; // фон → false (только попап), передний план → true
            _ui.GetPopup<UIPopupTimerDone>()?.ShowDone(name, ring);
        }
    }
}
