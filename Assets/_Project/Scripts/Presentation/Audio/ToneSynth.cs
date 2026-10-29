using System.Collections.Generic;
using UnityEngine;

namespace SheepStrike.Presentation.Audio
{
    /// <summary>
    /// Builds short sound effects from sine sweeps and noise at startup, so the game ships with zero audio
    /// files (mini-game first-package budget) and still sounds like a toy.
    /// </summary>
    public static class ToneSynth
    {
        public const int SampleRate = 22050;
        private const float AttackSeconds = 0.004f;

        public struct Tone
        {
            public float StartHz;
            public float EndHz;
            public float Seconds;
            public float Volume;
            public float Noise;
            public float Square;
        }

        public static AudioClip Clip(string name, params Tone[] notes)
        {
            var samples = new List<float>();
            foreach (var note in notes) Render(note, samples);
            var clip = AudioClip.Create(name, samples.Count, 1, SampleRate, false);
            clip.SetData(samples.ToArray(), 0);
            return clip;
        }

        public static Tone Sweep(float startHz, float endHz, float seconds, float volume) =>
            new Tone { StartHz = startHz, EndHz = endHz, Seconds = seconds, Volume = volume };

        private static void Render(Tone tone, List<float> samples)
        {
            int count = Mathf.CeilToInt(tone.Seconds * SampleRate);
            var noise = new System.Random(count);
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float hz = Mathf.Lerp(tone.StartHz, tone.EndHz, t);
                phase += hz / SampleRate;
                float sine = Mathf.Sin(phase * 2f * Mathf.PI);
                float wave = Mathf.Lerp(sine, Mathf.Sign(sine), tone.Square);
                float hiss = (float)noise.NextDouble() * 2f - 1f;
                float sample = Mathf.Lerp(wave, hiss, tone.Noise);
                samples.Add(sample * tone.Volume * Envelope(i, count));
            }
        }

        /// <summary>Instant attack and an exponential-ish fall, so every sound is a short pluck instead of a beep.</summary>
        private static float Envelope(int index, int count)
        {
            float attack = Mathf.Clamp01(index / (AttackSeconds * SampleRate));
            float decay = 1f - (float)index / count;
            return attack * decay * decay;
        }
    }
}
