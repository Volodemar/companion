using System;

namespace Companion.Core
{
    /// <summary>Время приёма в течение суток (одно на запись).</summary>
    public enum TimeOfDay
    {
        None = 0,
        Morning = 1, // утро
        Day = 2,     // день
        Evening = 3  // вечер
    }

    /// <summary>Связь приёма с едой (одна на запись).</summary>
    public enum MealRelation
    {
        None = 0,
        Before = 1,     // до еды
        After = 2,      // после еды
        During = 3,     // во время еды
        HourBefore = 4, // за час до еды
        HourAfter = 5,  // через час после еды
        Fasting = 6     // натощак
    }

    /// <summary>
    /// Одна строка таблицы приёма лекарств (сохраняется в JSON).
    /// id — GUID, уникален на строку: одно и то же лекарство можно завести дважды.
    /// takenDate — дата последнего зачёркивания «принято» (строка зачёркнута, если == сегодня);
    /// loggedDate — дата последней записи в историю по этой строке (для дедупа за день).
    /// </summary>
    [Serializable]
    public class MedicationData
    {
        public string id;
        public string name;
        public string dose;        // дозировка/объём, напр. «20 мг (1 капс)»
        public int timeOfDay;      // TimeOfDay
        public int meal;           // MealRelation
        public string note;        // доп. поле, ≤ 20 символов
        public string takenDate;   // yyyy-MM-dd последнего «принято» (для зачёркивания)
        public string loggedDate;  // yyyy-MM-dd последней записи в историю (для дедупа)

        public MedicationData() { }

        public MedicationData(string id, string name, string dose, int timeOfDay, int meal, string note)
        {
            this.id = id;
            this.name = name;
            this.dose = dose;
            this.timeOfDay = timeOfDay;
            this.meal = meal;
            this.note = note;
            this.takenDate = string.Empty;
            this.loggedDate = string.Empty;
        }

        /// <summary>Человекочитаемое время суток (пусто для None).</summary>
        public static string TimeOfDayRu(int value)
        {
            switch ((TimeOfDay)value)
            {
                case TimeOfDay.Morning: return "Утро";
                case TimeOfDay.Day: return "День";
                case TimeOfDay.Evening: return "Вечер";
                default: return string.Empty;
            }
        }

        /// <summary>Человекочитаемая связь с едой (пусто для None).</summary>
        public static string MealRu(int value)
        {
            switch ((MealRelation)value)
            {
                case MealRelation.Fasting: return "натощак";
                case MealRelation.Before: return "до еды";
                case MealRelation.After: return "после еды";
                case MealRelation.During: return "во время еды";
                case MealRelation.HourBefore: return "за час до еды";
                case MealRelation.HourAfter: return "через час после еды";
                default: return string.Empty;
            }
        }

        /// <summary>Порядок сортировки по времени суток: утро → день → вечер (None в конец).</summary>
        public static int TimeOfDaySortOrder(int value)
        {
            switch ((TimeOfDay)value)
            {
                case TimeOfDay.Morning: return 0;
                case TimeOfDay.Day: return 1;
                case TimeOfDay.Evening: return 2;
                default: return 3;
            }
        }

        /// <summary>
        /// Порядок сортировки по моменту еды:
        /// за час до → до еды → натощак → во время → после → через час после (None в конец).
        /// </summary>
        public static int MealSortOrder(int value)
        {
            switch ((MealRelation)value)
            {
                case MealRelation.HourBefore: return 0;
                case MealRelation.Before: return 1;
                case MealRelation.Fasting: return 2;
                case MealRelation.During: return 3;
                case MealRelation.After: return 4;
                case MealRelation.HourAfter: return 5;
                default: return 6;
            }
        }
    }
}
