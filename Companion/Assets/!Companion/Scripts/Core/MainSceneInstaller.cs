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

            // Точка входа приложения
            Container.BindInterfacesAndSelfTo<AppController>().AsSingle().NonLazy();
        }
    }
}
