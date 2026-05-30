using Zenject;
using Companion.UI;

namespace Companion.Core
{
    /// <summary>
    /// Точка входа приложения: после старта DI инициализирует UI
    /// и показывает главное окно. Аналог LevelController, но через
    /// штатный Zenject-хук IInitializable (без сцен-контроллера).
    /// </summary>
    public class AppController : IInitializable
    {
        [Inject] private UIManager _ui;

        public void Initialize()
        {
            _ui.Init();
            _ui.GetWindow<UIWindowMain>().Show();
        }
    }
}
