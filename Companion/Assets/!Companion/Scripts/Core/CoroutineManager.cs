using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Companion.Core
{
    /// <summary>
    /// Политика выполнения корутины при конфликте ключей
    /// </summary>
    public enum CoroutinePolicy
    {
        /// <summary>Пропустить запуск если корутина с таким ключом уже выполняется</summary>
        Skip,

        /// <summary>Остановить старую корутину и запустить новую</summary>
        Replace,

        /// <summary>Добавить в очередь (выполнится после завершения текущей)</summary>
        Queue,

        /// <summary>Запустить параллельно (игнорировать ключ)</summary>
        Parallel
    }

    /// <summary>
    /// Менеджер корутин с политиками выполнения и управлением жизненным циклом
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        // Словарь активных корутин по ключам
        private Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();

        // Очереди корутин по ключам (для политики Queue)
        private Dictionary<string, Queue<IEnumerator>> _queuedCoroutines = new Dictionary<string, Queue<IEnumerator>>();

        /// <summary>Количество активных корутин</summary>
        public int ActiveCoroutines => _activeCoroutines.Count;

        /// <summary>Количество корутин в очередях</summary>
        public int QueuedCoroutines => _queuedCoroutines.Values.Sum(q => q.Count);

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopAllManagedCoroutines();
        }

        // ============ ПУБЛИЧНЫЕ МЕТОДЫ С ПОЛИТИКАМИ ============

        /// <summary>
        /// Запустить корутину с политикой Replace (остановить старую, запустить новую)
        /// Ключ автоматически извлекается из имени метода корутины
        /// </summary>
        public string CoroutineReplace(IEnumerator coroutine)
        {
            return RunCoroutine(coroutine, CoroutinePolicy.Replace, null);
        }

        /// <summary>
        /// Запустить корутину с политикой Skip (пропустить если уже выполняется)
        /// Ключ автоматически извлекается из имени метода корутины
        /// </summary>
        /// <param name="coroutine">Корутина для запуска</param>
        /// <param name="onSkip">Callback результата: false - запустилась, true - пропущена</param>
        public string CoroutineSkip(IEnumerator coroutine, System.Action<bool> onSkip = null)
        {
            return RunCoroutine(coroutine, CoroutinePolicy.Skip, onSkip);
        }

        /// <summary>
        /// Запустить корутину с политикой Queue (добавить в очередь)
        /// Ключ автоматически извлекается из имени метода корутины
        /// </summary>
        public string CoroutineQueue(IEnumerator coroutine)
        {
            return RunCoroutine(coroutine, CoroutinePolicy.Queue, null);
        }

        /// <summary>
        /// Запустить корутину параллельно (всегда запускается, ключ генерируется автоматически)
        /// </summary>
        public string CoroutineParallel(IEnumerator coroutine)
        {
            return RunCoroutine(coroutine, CoroutinePolicy.Parallel, null);
        }

        // ============ ОСНОВНОЙ МЕТОД ============

        private string RunCoroutine(IEnumerator coroutine, CoroutinePolicy policy, System.Action<bool> onSkip = null)
        {
            string key;

            if (policy == CoroutinePolicy.Parallel)
            {
                // Для Parallel всегда генерируем уникальный ключ
                key = System.Guid.NewGuid().ToString();
            }
            else
            {
                // Для Replace/Skip/Queue извлекаем имя метода
                key = ExtractCoroutineMethodName(coroutine);

                if (string.IsNullOrEmpty(key))
                {
                    throw new System.Exception($"CoroutineManager: не удалось извлечь имя метода из корутины типа '{coroutine.GetType().Name}'. Убедитесь что передаете корутину-метод, а не анонимную функцию.");
                }
            }

            bool isRunning = _activeCoroutines.ContainsKey(key);

            switch (policy)
            {
                case CoroutinePolicy.Skip:
                    if (isRunning)
                    {
                        // Корутина пропущена - вызываем callback с true
                        onSkip?.Invoke(true);
                        return null;
                    }
                    break;

                case CoroutinePolicy.Replace:
                    if (isRunning)
                        StopCoroutineByKey(key);
                    break;

                case CoroutinePolicy.Queue:
                    if (isRunning)
                    {
                        if (!_queuedCoroutines.ContainsKey(key))
                            _queuedCoroutines[key] = new Queue<IEnumerator>();

                        _queuedCoroutines[key].Enqueue(coroutine);
                        return key;
                    }
                    break;

                case CoroutinePolicy.Parallel:
                    key = System.Guid.NewGuid().ToString();
                    break;
            }

            Coroutine newCoroutine = StartCoroutine(CoroutineWrapper(key, coroutine));
            _activeCoroutines[key] = newCoroutine;

            // Корутина запустилась - вызываем callback с false
            onSkip?.Invoke(false);

            return key;
        }

        private IEnumerator CoroutineWrapper(string key, IEnumerator coroutine)
        {
            yield return coroutine;

            if (_activeCoroutines.ContainsKey(key))
                _activeCoroutines.Remove(key);

            if (_queuedCoroutines.ContainsKey(key) && _queuedCoroutines[key].Count > 0)
            {
                IEnumerator nextCoroutine = _queuedCoroutines[key].Dequeue();
                RunCoroutineWithKey(nextCoroutine, CoroutinePolicy.Queue, key);
            }
        }

        /// <summary>
        /// Запустить корутину с явным ключом (для очередей)
        /// </summary>
        private string RunCoroutineWithKey(IEnumerator coroutine, CoroutinePolicy policy, string key)
        {
            bool isRunning = _activeCoroutines.ContainsKey(key);

            if (policy == CoroutinePolicy.Replace && isRunning)
            {
                StopCoroutineByKey(key);
            }

            Coroutine newCoroutine = StartCoroutine(CoroutineWrapper(key, coroutine));
            _activeCoroutines[key] = newCoroutine;

            return key;
        }

        // ============ УПРАВЛЕНИЕ КОРУТИНАМИ ============

        /// <summary>
        /// Остановить корутину по ключу
        /// </summary>
        public void StopCoroutineByKey(string key)
        {
            if (_activeCoroutines.ContainsKey(key))
            {
                StopCoroutine(_activeCoroutines[key]);
                _activeCoroutines.Remove(key);
            }

            if (_queuedCoroutines.ContainsKey(key))
            {
                _queuedCoroutines[key].Clear();
                _queuedCoroutines.Remove(key);
            }
        }

        /// <summary>
        /// Проверить выполняется ли корутина с указанным ключом
        /// </summary>
        public bool IsRunning(string key)
        {
            return _activeCoroutines.ContainsKey(key);
        }

        /// <summary>
        /// Получить количество корутин в очереди для указанного ключа
        /// </summary>
        public int GetQueuedCountForKey(string key)
        {
            if (_queuedCoroutines.ContainsKey(key))
                return _queuedCoroutines[key].Count;
            return 0;
        }

        /// <summary>
        /// Остановить все управляемые корутины
        /// </summary>
        public void StopAllManagedCoroutines()
        {
            foreach (var kvp in _activeCoroutines)
            {
                if (kvp.Value != null)
                    StopCoroutine(kvp.Value);
            }

            _activeCoroutines.Clear();
            _queuedCoroutines.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StopAllManagedCoroutines();
        }

        // ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============

        /// <summary>
        /// Извлечь имя метода из корутины
        /// Компилятор C# создает класс вроде "<MethodName>d__123"
        /// </summary>
        private string ExtractCoroutineMethodName(IEnumerator coroutine)
        {
            var type = coroutine.GetType();
            var typeName = type.Name;

            // Ищем имя между < и >
            if (typeName.Contains("<") && typeName.Contains(">"))
            {
                int start = typeName.IndexOf('<') + 1;
                int end = typeName.IndexOf('>');
                if (end > start)
                {
                    return typeName.Substring(start, end - start);
                }
            }

            return null;
        }
    }
}
