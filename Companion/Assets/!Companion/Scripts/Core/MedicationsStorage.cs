using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Локальное хранилище таблицы лекарств: список строк в JSON-файле
    /// (Application.persistentDataPath/medications.json). Биндится как синглтон Zenject.
    /// Зачёркивания «принято» сбрасываются сами по смене дня — состояние хранится датой,
    /// отдельный сброс не нужен.
    /// </summary>
    public class MedicationsStorage
    {
        // Обёртка нужна, т.к. JsonUtility не сериализует голый List.
        [Serializable]
        private class SaveData
        {
            public List<MedicationData> items = new List<MedicationData>();
        }

        private SaveData _data = new SaveData();

        private string FilePath => Path.Combine(Application.persistentDataPath, "medications.json");

        private static string Today => DateTime.Now.ToString("yyyy-MM-dd");

        public IReadOnlyList<MedicationData> Items => _data.items;

        public MedicationsStorage()
        {
            Load();
        }

        /// <summary>Создаёт и сохраняет новую строку лекарства, возвращает её.</summary>
        public MedicationData Add(string name, string dose, int timeOfDay, int meal, string note)
        {
            var med = new MedicationData(Guid.NewGuid().ToString("N"), name, dose, timeOfDay, meal, note);
            _data.items.Add(med);
            Save();
            return med;
        }

        public void Remove(string id)
        {
            _data.items.RemoveAll(m => m.id == id);
            Save();
        }

        /// <summary>Зачёркнута ли строка сегодня (то есть отмечена как принятая).</summary>
        public bool IsTakenToday(MedicationData m)
        {
            return m != null && m.takenDate == Today;
        }

        /// <summary>
        /// Отметить строку принятой на сегодня. Возвращает true, если за сегодня запись в
        /// историю по этой строке ещё не делалась (значит, её нужно записать) — иначе false.
        /// </summary>
        public bool MarkTaken(string id)
        {
            var med = Find(id);
            if (med == null)
                return false;

            med.takenDate = Today;

            bool needLog = med.loggedDate != Today;
            if (needLog)
                med.loggedDate = Today;

            Save();
            return needLog;
        }

        /// <summary>Снять отметку «принято» (loggedDate не трогаем — дедуп за день сохраняется).</summary>
        public void Unmark(string id)
        {
            var med = Find(id);
            if (med == null)
                return;

            med.takenDate = string.Empty;
            Save();
        }

        private MedicationData Find(string id)
        {
            return _data.items.Find(m => m.id == id);
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
                Debug.LogError($"Error: Ошибка загрузки лекарств: {e.Message}");
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
                Debug.LogError($"Error: Ошибка сохранения лекарств: {e.Message}");
            }
        }
    }
}
