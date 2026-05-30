using Zenject;
using UnityEngine;
using Companion.UI;

namespace Companion.Core
{
    /// <summary>
    /// Инсталляция зависимостей сцены приложения (одна сцена, без ProjectContext).
    /// </summary>
    public class MainSceneInstaller : MonoInstaller
    {
        [Header("SceneObjects")]
        [SerializeField] private UIManager uiManager;

        public override void InstallBindings()
        {
            // CoroutineManager как глобальный синглтон сцены
            Container.Bind<CoroutineManager>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("CoroutineManager")
                .AsSingle()
                .NonLazy();

            // UIManager берём из объекта сцены
            Container.Bind<UIManager>().FromComponentOn(uiManager.gameObject).AsSingle();

            // Хранилище и сервис таймеров
            Container.Bind<TimersStorage>().AsSingle();
            Container.Bind<TimerService>().AsSingle().NonLazy();

            // Звук: создаётся на отдельном GameObject, beep генерируется в коде
            Container.Bind<AudioManager>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("AudioManager")
                .AsSingle()
                .NonLazy();

            // Мост сворачивания/возврата приложения (фоновый будильник — нативный плагин)
            Container.Bind<TimerBackgroundService>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("TimerBackgroundService")
                .AsSingle()
                .NonLazy();

            // Точка входа приложения
            Container.BindInterfacesAndSelfTo<AppController>().AsSingle().NonLazy();

            // Слушатель завершения таймеров → показ попапа (чтобы попап мог быть выключен в сцене)
            Container.BindInterfacesAndSelfTo<TimerPopupController>().AsSingle().NonLazy();
        }
    }
}
