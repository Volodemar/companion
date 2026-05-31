using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Хранилище ИДУЩИХ таймеров (а не их определений — те в TimersStorage).
    /// Запоминает на диск пары «id таймера → момент завершения (UTC)», чтобы отсчёт
    /// переживал закрытие/перезапуск приложения: при старте TimerService восстановит
    /// по этим записям ещё идущие таймеры и до-завершит истёкшие. Биндится как синглтон Zenject.
    /// </summary>
    public class TimerRunsStorage
    {
        [Serializable]
        public class Run
        {
            public int timerId;
            public string endUtcIso; // DateTime завершения в UTC, формат "o" (round-trip)
        }

        // Обёртка нужна, т.к. JsonUtility не сериализует голый List.
        [Serializable]
        private class SaveData
        {
            public List<Run> runs = new List<Run>();
        }

        private SaveData _data = new SaveData();

        private string FilePath => Path.Combine(Application.persistentDataPath, "timer_runs.json");

        public IReadOnlyList<Run> Runs => _data.runs;

        public TimerRunsStorage()
        {
            Load();
        }

        /// <summary>Запомнить идущий таймер (или обновить момент завершения, если уже есть).</summary>
        public void StartRun(int timerId, DateTime endUtc)
        {
            string iso = endUtc.ToUniversalTime().ToString("o");
            var existing = _data.runs.Find(r => r.timerId == timerId);
            if (existing != null)
                existing.endUtcIso = iso;
            else
                _data.runs.Add(new Run { timerId = timerId, endUtcIso = iso });
            Save();
        }

        /// <summary>Убрать запись об идущем таймере (остановка/завершение).</summary>
        public void EndRun(int timerId)
        {
            int removed = _data.runs.RemoveAll(r => r.timerId == timerId);
            if (removed > 0)
                Save();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var loaded = JsonUtility.FromJson<SaveData>(json);
                    if (loaded != null && loaded.runs != null)
                        _data = loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: Ошибка загрузки идущих таймеров: {e.Message}");
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: Ошибка сохранения идущих таймеров: {e.Message}");
            }
        }
    }
}
