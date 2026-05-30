using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Локальное хранилище таймеров: загрузка/сохранение списка в JSON-файл
    /// (Application.persistentDataPath/timers.json). Биндится как синглтон Zenject.
    /// </summary>
    public class TimersStorage
    {
        // Обёртка нужна, т.к. JsonUtility не сериализует голый List.
        [Serializable]
        private class SaveData
        {
            public int nextId = 1;
            public List<TimerData> items = new List<TimerData>();
        }

        private SaveData _data = new SaveData();

        private string FilePath => Path.Combine(Application.persistentDataPath, "timers.json");

        public IReadOnlyList<TimerData> Timers => _data.items;

        public TimersStorage()
        {
            Load();
        }

        /// <summary>Создаёт и сохраняет новый таймер, возвращает его.</summary>
        public TimerData Add(string name, int minutes)
        {
            var timer = new TimerData(_data.nextId++, name, minutes);
            _data.items.Add(timer);
            Save();
            return timer;
        }

        public void Remove(int id)
        {
            _data.items.RemoveAll(t => t.id == id);
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
                    if (loaded != null)
                        _data = loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: Ошибка загрузки таймеров: {e.Message}");
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
                Debug.LogError($"Error: Ошибка сохранения таймеров: {e.Message}");
            }
        }
    }
}
