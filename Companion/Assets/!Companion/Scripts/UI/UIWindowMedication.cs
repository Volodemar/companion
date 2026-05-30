using System.Collections.Generic;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Companion.UI
{
    /// <summary>
    /// Окно раздела «Лекарства»: кнопка «Добавить» и таблица строк приёма.
    /// Тап по строке отмечает приём (зачёркивание) и пишет факт в историю; свайп влево
    /// открывает кнопку «Удалить». Зачёркивания сбрасываются с началом нового дня.
    /// </summary>
    public class UIWindowMedication : UIWindow
    {
        [SerializeField] private Button buttonAdd;
        [SerializeField] private Button buttonBack;
        [SerializeField] private RectTransform content;                // контейнер списка
        [SerializeField] private MedicationRow medicationRowPrefab;    // шаблон строки

        [Inject] private MedicationsStorage _storage;
        [Inject] private MedicationHistoryStorage _history;

        private readonly List<MedicationRow> _rows = new List<MedicationRow>();

        private void Awake()
        {
            buttonAdd.onClick.AddListener(OnAddClicked);
            if (buttonBack != null)
                buttonBack.onClick.AddListener(OnBackClicked);
        }

        protected override void OnShow()
        {
            base.OnShow();
            RebuildList();
        }

        private void RebuildList()
        {
            foreach (var row in _rows)
                Destroy(row.gameObject);
            _rows.Clear();

            // Сортировка: по времени суток (утро→день→вечер), внутри — по моменту еды
            // (натощак→за час до→до еды→во время→после→через час после).
            var meds = new List<MedicationData>(_storage.Items);
            meds.Sort((a, b) =>
            {
                int byTime = MedicationData.TimeOfDaySortOrder(a.timeOfDay)
                    .CompareTo(MedicationData.TimeOfDaySortOrder(b.timeOfDay));
                return byTime != 0
                    ? byTime
                    : MedicationData.MealSortOrder(a.meal).CompareTo(MedicationData.MealSortOrder(b.meal));
            });

            foreach (var med in meds)
            {
                var row = Instantiate(medicationRowPrefab, content);
                row.gameObject.SetActive(true);

                var captured = med; // замыкание на конкретную строку
                row.Setup(
                    captured,
                    onTapTaken: () => OnRowTapped(captured, row),
                    onDelete: () =>
                    {
                        _storage.Remove(captured.id);
                        RebuildList();
                    });

                row.SetStruck(_storage.IsTakenToday(captured));
                _rows.Add(row);
            }
        }

        // Тап по строке: переключает «принято». Первое зачёркивание за день пишет историю.
        private void OnRowTapped(MedicationData med, MedicationRow row)
        {
            if (_storage.IsTakenToday(med))
            {
                _storage.Unmark(med.id);
                row.SetStruck(false);
            }
            else
            {
                bool needLog = _storage.MarkTaken(med.id);
                row.SetStruck(true);
                if (needLog)
                    _history.Append(med);
            }
        }

        private void OnAddClicked()
        {
            _ui.GetPopup<UIPopupMedicationCreate>().Open(onCreated: RebuildList);
        }

        private void OnBackClicked()
        {
            _ui.GetWindow<UIWindowMain>().Show();
        }
    }
}
