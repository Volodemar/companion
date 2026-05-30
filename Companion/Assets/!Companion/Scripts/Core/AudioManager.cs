using UnityEngine;

namespace Companion.Core
{
    /// <summary>
    /// Проигрыватель сигнала-будильника. Сигнал — периодический бип (тон + тишина),
    /// зациклен, генерируется в коде (без аудио-ассета). Источник один и общий,
    /// поэтому при нескольких одновременно завершившихся таймерах звук не накладывается —
    /// это один будильник, который звенит, пока его не остановят.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private AudioSource _source;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.clip = CreateBeepLoop();
        }

        /// <summary>Запустить периодический сигнал (если ещё не звучит).</summary>
        public void StartAlarm()
        {
            if (_source != null && !_source.isPlaying)
                _source.Play();
        }

        /// <summary>Остановить сигнал.</summary>
        public void StopAlarm()
        {
            if (_source != null)
                _source.Stop();
        }

        // Клип «тон ~0.4 c + тишина до 1 c», зациклен → бип примерно раз в секунду.
        private AudioClip CreateBeepLoop()
        {
            const int sampleRate = 44100;
            const float toneDuration = 0.4f;
            const float totalDuration = 1.0f;
            const float frequency = 880f;

            int total = (int)(sampleRate * totalDuration);
            int toneCount = (int)(sampleRate * toneDuration);
            var samples = new float[total];

            for (int i = 0; i < toneCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - t / toneDuration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.5f * envelope;
            }
            // хвост массива остаётся нулевым — это пауза между бипами

            var clip = AudioClip.Create("AlarmBeepLoop", total, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
