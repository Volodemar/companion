using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Простой проигрыватель звуков. Сигнал завершения таймера генерируется
    /// программно (синус с затуханием) — отдельный аудио-ассет не нужен.
    /// Подписан на EventManager: играет beep по TimerCompleted.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private AudioSource _source;
        private AudioClip _beep;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _beep = CreateBeep();
            EventManager.OnAction += OnEvent;
        }

        private void OnDestroy()
        {
            EventManager.OnAction -= OnEvent;
        }

        private void OnEvent(int id, object obj, object obj2)
        {
            if (id == EventManager.TimerCompleted)
                PlayBeep();
        }

        public void PlayBeep()
        {
            if (_source != null && _beep != null)
                _source.PlayOneShot(_beep);
        }

        // Короткий сигнал ~0.4 c, 880 Гц, с линейным затуханием.
        private AudioClip CreateBeep()
        {
            const int sampleRate = 44100;
            const float duration = 0.4f;
            const float frequency = 880f;

            int count = (int)(sampleRate * duration);
            var samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.5f * envelope;
            }

            var clip = AudioClip.Create("BeepTimerDone", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
