using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Журнал фактических приёмов лекарств (что и когда было отмечено «принято»).
    /// Пишется в Application.persistentDataPath/medication_history.json. Дедуп за день
    /// обеспечивает MedicationsStorage.MarkTaken — сюда добавляем только когда нужно.
    /// Используется позже для просмотра истории с фильтрацией.
    /// </summary>
    public class MedicationHistoryStorage
    {
        /// <summary>Одна запись о приёме.</summary>
        [Serializable]
        public class HistoryEntry
        {
            public string recordId;  // id строки лекарства (у одинаковых лекарств разные id)
            public string name;
            public string dose;
            public int timeOfDay;    // TimeOfDay
            public int meal;         // MealRelation
            public string note;
            public string dateFull;  // напр. «30.05.2026»
            public string timeShort; // напр. «14:35»
            public string iso;       // машинная метка времени для сортировки/фильтрации
        }

        [Serializable]
        private class SaveData
        {
            public List<HistoryEntry> entries = new List<HistoryEntry>();
        }

        private SaveData _data = new SaveData();

        private string FilePath => Path.Combine(Application.persistentDataPath, "medication_history.json");

        public IReadOnlyList<HistoryEntry> Entries => _data.entries;

        public MedicationHistoryStorage()
        {
            Load();
        }

        /// <summary>Записать факт приёма лекарства (текущие дата и время).</summary>
        public void Append(MedicationData m)
        {
            if (m == null)
                return;

            var now = DateTime.Now;
            _data.entries.Add(new HistoryEntry
            {
                recordId = m.id,
                name = m.name,
                dose = m.dose,
                timeOfDay = m.timeOfDay,
                meal = m.meal,
                note = m.note,
                dateFull = now.ToString("dd.MM.yyyy"),
                timeShort = now.ToString("HH:mm"),
                iso = now.ToString("o")
            });
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
                Debug.LogError($"Error: Ошибка загрузки истории приёма: {e.Message}");
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
                Debug.LogError($"Error: Ошибка сохранения истории приёма: {e.Message}");
            }
        }
    }
}
