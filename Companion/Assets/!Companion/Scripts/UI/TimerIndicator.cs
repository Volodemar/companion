using Companion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Companion.UI
{
    /// <summary>
    /// Кружок-индикатор одного идущего таймера: показывает остаток мм:сс.
    /// </summary>
    public class TimerIndicator : MonoBehaviour
    {
        [SerializeField] private Text label;

        public void Setup(TimerData timer)
        {
            SetRemaining(timer.minutes * 60);
        }

        public void SetRemaining(int seconds)
        {
            int m = seconds / 60;
            int s = seconds % 60;
            label.text = $"{m:00}:{s:00}";
        }
    }
}
