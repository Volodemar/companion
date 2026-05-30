using System;
using Companion.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Companion.UI
{
    /// <summary>
    /// Попап создания таймера: название и длительность в минутах.
    /// </summary>
    public class UIPopupTimerCreate : UIPopup
    {
        [SerializeField] private InputField inputName;
        [SerializeField] private InputField inputMinutes;
        [SerializeField] private Button buttonCreate;
        [SerializeField] private Button buttonCancel;

        [Inject] private TimersStorage _storage;

        private Action _onCreated;

        private void Awake()
        {
            buttonCreate.onClick.AddListener(OnCreateClicked);
            buttonCancel.onClick.AddListener(Hide);
        }

        /// <summary>Открыть попап создания; onCreated вызовется после успешного создания.</summary>
        public void Open(Action onCreated)
        {
            _onCreated = onCreated;
            inputName.text = string.Empty;
            inputMinutes.text = string.Empty;
            Show();
        }

        private void OnCreateClicked()
        {
            string name = string.IsNullOrWhiteSpace(inputName.text) ? "Таймер" : inputName.text.Trim();

            if (!int.TryParse(inputMinutes.text, out int minutes) || minutes <= 0)
            {
                Debug.Log("[таймер] Некорректная длительность — создание отменено");
                return;
            }

            _storage.Add(name, minutes);
            _onCreated?.Invoke();
            Hide();
        }
    }
}
