using System;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Companion.UI
{
    /// <summary>
    /// Попап добавления строки лекарства: название, дозировка, время суток (одно из трёх),
    /// связь с едой (одна из пяти) и доп. поле (≤ 20 символов). Время и еда выбираются
    /// чип-кнопками как радио-группы (выбранная подсвечивается).
    /// </summary>
    public class UIPopupMedicationCreate : UIPopup
    {
        [SerializeField] private InputField inputName;
        [SerializeField] private InputField inputDose;
        [SerializeField] private InputField inputNote;

        // 3 чипа времени суток (порядок = TimeValues)
        [SerializeField] private Button[] timeButtons;
        // 6 чипов связи с едой (порядок = MealValues)
        [SerializeField] private Button[] mealButtons;

        [SerializeField] private Button buttonCreate;
        [SerializeField] private Button buttonCancel;

        [Inject] private MedicationsStorage _storage;

        // Цвета чипов: выбран / не выбран.
        private static readonly Color SelectedColor = new Color(0.20f, 0.55f, 0.90f, 1f);
        private static readonly Color NormalColor = new Color(0.25f, 0.25f, 0.28f, 1f);

        private const int NoteLimit = 20;

        // Соответствие индекса чипа значению enum (порядок чипов в префабе должен совпадать).
        private static readonly int[] TimeValues =
        {
            (int)TimeOfDay.Morning, (int)TimeOfDay.Day, (int)TimeOfDay.Evening,
        };
        private static readonly int[] MealValues =
        {
            (int)MealRelation.Fasting,    // 0 — по умолчанию
            (int)MealRelation.HourBefore, // 1
            (int)MealRelation.Before,     // 2
            (int)MealRelation.During,     // 3
            (int)MealRelation.After,      // 4
            (int)MealRelation.HourAfter,  // 5
        };
        private const int DefaultMeal = 0; // «Натощак»

        private Action _onCreated;
        private int _selTime = -1;         // индекс выбранного чипа времени (-1 = не выбрано)
        private int _selMeal = DefaultMeal; // индекс выбранного чипа связи с едой

        private void Awake()
        {
            buttonCreate.onClick.AddListener(OnCreateClicked);
            buttonCancel.onClick.AddListener(Hide);

            if (inputNote != null)
                inputNote.characterLimit = NoteLimit;

            for (int i = 0; i < timeButtons.Length; i++)
            {
                int index = i;
                timeButtons[i].onClick.AddListener(() => SelectTime(index));
            }
            for (int i = 0; i < mealButtons.Length; i++)
            {
                int index = i;
                mealButtons[i].onClick.AddListener(() => SelectMeal(index));
            }
        }

        /// <summary>Открыть попап; onCreated вызовется после успешного создания.</summary>
        public void Open(Action onCreated)
        {
            _onCreated = onCreated;
            inputName.text = string.Empty;
            inputDose.text = string.Empty;
            inputNote.text = string.Empty;
            _selTime = -1;
            _selMeal = DefaultMeal; // «Натощак» предвыбран
            RefreshChips();
            Show();
        }

        private void SelectTime(int index)
        {
            // Повторный тап по выбранному чипу снимает выбор.
            _selTime = _selTime == index ? -1 : index;
            RefreshChips();
        }

        private void SelectMeal(int index)
        {
            _selMeal = _selMeal == index ? -1 : index;
            RefreshChips();
        }

        private void RefreshChips()
        {
            for (int i = 0; i < timeButtons.Length; i++)
                Paint(timeButtons[i], i == _selTime);
            for (int i = 0; i < mealButtons.Length; i++)
                Paint(mealButtons[i], i == _selMeal);
        }

        private static void Paint(Button button, bool selected)
        {
            if (button != null && button.image != null)
                button.image.color = selected ? SelectedColor : NormalColor;
        }

        private void OnCreateClicked()
        {
            // Без названия и без выбранного времени суток попап не закрываем.
            if (string.IsNullOrWhiteSpace(inputName.text) || _selTime < 0)
            {
                Debug.Log("[лекарства] Нужно указать название и время суток — создание отменено");
                return;
            }

            string name = inputName.text.Trim();
            string dose = inputDose.text.Trim();
            string note = inputNote.text.Trim();

            // Индекс чипа → значение enum.
            int timeOfDay = TimeValues[_selTime];
            int meal = _selMeal < 0 ? (int)MealRelation.None : MealValues[_selMeal];

            _storage.Add(name, dose, timeOfDay, meal, note);
            _onCreated?.Invoke();
            Hide();
        }
    }
}
