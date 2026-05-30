using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Строка списка со свайпом влево: сдвигает передний слой и открывает спрятанную
    /// справа область (например, кнопку «Удалить»). Вертикальные жесты пробрасываются в
    /// родительский ScrollRect (прокрутка списка), горизонтальные — двигают слой.
    /// Тап по закрытой строке вызывает TapWhenClosed; тап по открытой — закрывает её.
    /// Компонент общий — годится и для списка таймеров, и для таблицы лекарств.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SwipeRow : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform foreground;
        [SerializeField] private float revealWidth = 240f;

        /// <summary>Действие по тапу, когда строка закрыта (для таймера — запуск).</summary>
        public Action TapWhenClosed;

        private ScrollRect _scroll;
        private Canvas _canvas;
        private bool _routingToScroll;
        private bool _dragged;
        private bool _isOpen;

        private void Awake()
        {
            _scroll = GetComponentInParent<ScrollRect>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            _dragged = false;
        }

        public void OnBeginDrag(PointerEventData e)
        {
            _dragged = true;

            // Решаем направление: горизонталь — наш свайп, вертикаль — прокрутка списка.
            _routingToScroll = Mathf.Abs(e.delta.x) < Mathf.Abs(e.delta.y);
            if (_routingToScroll && _scroll != null)
                _scroll.OnBeginDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_routingToScroll)
            {
                if (_scroll != null) _scroll.OnDrag(e);
                return;
            }

            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            float x = foreground.anchoredPosition.x + e.delta.x / Mathf.Max(scale, 0.0001f);
            SetX(Mathf.Clamp(x, -revealWidth, 0f));
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (_routingToScroll)
            {
                if (_scroll != null) _scroll.OnEndDrag(e);
                _routingToScroll = false;
                return;
            }

            // Доводим до открытого/закрытого по тому, дальше ли половины утянули.
            SetOpen(foreground.anchoredPosition.x < -revealWidth * 0.5f);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (_dragged) // это был свайп, а не тап
                return;

            if (_isOpen)
                SetOpen(false);
            else
                TapWhenClosed?.Invoke();
        }

        private void SetOpen(bool open)
        {
            _isOpen = open;
            SetX(open ? -revealWidth : 0f);
        }

        private void SetX(float x)
        {
            if (foreground != null)
                foreground.anchoredPosition = new Vector2(x, foreground.anchoredPosition.y);
        }
    }
}
