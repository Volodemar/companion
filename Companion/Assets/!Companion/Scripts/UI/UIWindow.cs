using System;
using System.Collections;
using Companion.Core;
using UnityEngine;

namespace Companion.UI
{
    /// <summary>
    /// Базовый класс для всех окон в приложении.
    /// </summary>
    public class UIWindow : BaseGameObject
    {
        public void Show(Action onComplete = null)
        {
            _ui.HideAllUIWindows(except: this);
            OnShow();

            _coroutineManager.CoroutineReplace(AnimationFadeIn(onComplete));
        }

        /// <summary>
        /// Корутинная версия Show с ожиданием завершения анимации.
        /// </summary>
        public IEnumerator ShowAnimated()
        {
            bool complete = false;
            Show(() => complete = true);
            yield return new WaitUntil(() => complete);
        }

        public void Hide(Action onComplete = null)
        {
            _coroutineManager.CoroutineReplace(AnimationFadeOut(onComplete));
        }

        /// <summary>
        /// Корутинная версия Hide с ожиданием завершения анимации.
        /// </summary>
        public IEnumerator HideAnimated()
        {
            bool complete = false;
            Hide(() => complete = true);
            yield return new WaitUntil(() => complete);
        }

        protected virtual void OnShow()
        {
            gameObject.SetActive(true);
        }

        protected virtual void OnHide()
        {
            gameObject.SetActive(false);
        }

        protected virtual IEnumerator AnimationFadeIn(Action onComplete)
        {
            yield return null;
            onComplete?.Invoke();
        }

        protected virtual IEnumerator AnimationFadeOut(Action onComplete)
        {
            yield return null;
            OnHide();
            onComplete?.Invoke();
        }
    }
}
