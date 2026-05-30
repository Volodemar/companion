using System;

namespace Companion.Core
{
    /// <summary>
    /// Данные одного пользовательского таймера (сохраняются в JSON).
    /// </summary>
    [Serializable]
    public class TimerData
    {
        public int id;
        public string name;
        public int minutes;

        public TimerData() { }

        public TimerData(int id, string name, int minutes)
        {
            this.id = id;
            this.name = name;
            this.minutes = minutes;
        }
    }
}
