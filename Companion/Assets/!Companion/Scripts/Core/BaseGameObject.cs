using Zenject;
using UnityEngine;
using Companion.UI;

namespace Companion.Core
{
    /// <summary>
    /// Базовый объект с доступом к основным менеджерам через DI.
    /// Обрезанная версия: пока нужны только менеджер корутин и UI.
    /// </summary>
    public class BaseGameObject : MonoBehaviour
    {
        [Inject] protected CoroutineManager _coroutineManager;
        [Inject] protected UIManager _ui;
    }
}
