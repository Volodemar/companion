using System;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Строка таблицы лекарств (две строки текста): шапка «время приёма | препарат» и
    /// подпись «дозировка · доп.поле». Тап по строке (через SwipeRow) отмечает приём
    /// (зачёркивание линией + серый текст), свайп влево открывает кнопку «Удалить».
    /// Создаётся клонированием шаблона, Zenject-инъекции не требует.
    /// </summary>
    public class MedicationRow : MonoBehaviour
    {
        [SerializeField] private Text timeText;      // время приёма (слева сверху)
        [SerializeField] private Text nameText;      // препарат (справа сверху)
        [SerializeField] private Text subtitleText;  // дозировка · доп.поле (снизу)
        [SerializeField] private Image strikeLine;   // линия зачёркивания (вкл/выкл)
        [SerializeField] private Button deleteButton;
        [SerializeField] private SwipeRow swipeRow;

        // Цвета текста: обычный и приглушённый (когда «принято»).
        private static readonly Color NormalColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        private static readonly Color StruckColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        public void Setup(MedicationData m, Action onTapTaken, Action onDelete)
        {
            timeText.text = BuildTimeText(m);
            nameText.text = m.name;
            subtitleText.text = BuildSubtitle(m);

            swipeRow.TapWhenClosed = onTapTaken;

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDelete?.Invoke());
        }

        /// <summary>Включить/выключить вид «принято»: линия поверх + приглушённый текст.</summary>
        public void SetStruck(bool on)
        {
            if (strikeLine != null)
                strikeLine.enabled = on;

            Color c = on ? StruckColor : NormalColor;
            timeText.color = c;
            nameText.color = c;
            subtitleText.color = c;
        }

        // «Утро · после еды» (пустые части опускаем).
        private static string BuildTimeText(MedicationData m)
        {
            string time = MedicationData.TimeOfDayRu(m.timeOfDay);
            string meal = MedicationData.MealRu(m.meal);

            if (string.IsNullOrEmpty(time)) return meal;
            if (string.IsNullOrEmpty(meal)) return time;
            return $"{time} · {meal}";
        }

        // «20 мг (1 капс) · вскрывать нельзя» (доп.поле опускаем, если пустое).
        private static string BuildSubtitle(MedicationData m)
        {
            string dose = m.dose ?? string.Empty;
            if (string.IsNullOrWhiteSpace(m.note))
                return dose;
            return string.IsNullOrWhiteSpace(dose) ? m.note : $"{dose} · {m.note}";
        }
    }
}
