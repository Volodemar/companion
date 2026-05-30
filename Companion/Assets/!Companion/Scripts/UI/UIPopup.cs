using Companion.Core;
using UnityEngine;

namespace Companion.UI
{
    /// <summary>
    /// Базовый класс для всех попапов в UI.
    /// </summary>
    public class UIPopup : BaseGameObject
    {
        [SerializeField] private bool isCanHide = true;
        public bool IsCanHide => isCanHide;

        public void Show()
        {
            gameObject.SetActive(true);

            OnShow();
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            OnHide();
        }

        protected virtual void OnShow()
        {

        }

        protected virtual void OnHide()
        {

        }
    }
}
