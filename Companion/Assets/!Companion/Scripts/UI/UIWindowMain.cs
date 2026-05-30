using UnityEngine;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Главное окно приложения: чёрный фон и кнопки разделов.
    /// На старте два раздела — «Таймер» и «Лекарства».
    /// </summary>
    public class UIWindowMain : UIWindow
    {
        [SerializeField] private Button buttonTimer;
        [SerializeField] private Button buttonMedications;

        private void Awake()
        {
            buttonTimer.onClick.AddListener(OnTimerClicked);
            buttonMedications.onClick.AddListener(OnMedicationsClicked);
        }

        private void OnTimerClicked()
        {
            _ui.GetWindow<UIWindowTimer>().Show();
        }

        private void OnMedicationsClicked()
        {
            // Заглушка: раздел «Лекарства» ещё не реализован
            Debug.Log("[ui] Нажата кнопка «Лекарства»");
        }
    }
}
