using System.Collections.Generic;
using UnityEngine;

namespace SheepStrike.Presentation.Audio
{
    /// <summary>
    /// A short, cheerful background loop synthesised at startup: a plucky pentatonic melody over a bouncing bass,
    /// eight bars at 112 bpm. No audio file ships with the game.
    /// </summary>
    public static class MusicLoop
    {
        private const float Bpm = 112f;
        private const float MelodyVolume = 0.16f;
        private const float BassVolume = 0.14f;

        // Semitones from C5 per eighth note; -100 is a rest. Pentatonic, so nothing ever clashes.
        private static readonly int[] Melody =
        {
            0, 4, 7, 9, 7, 4, 2, -100,   4, 7, 9, 12, 9, 7, 4, -100,
            2, 4, 7, 4, 2, 0, -3, -100,  0, 2, 4, 7, 4, 2, 0, -100,
            0, 4, 7, 9, 12, 9, 7, 4,     9, 7, 4, 7, 9, 12, 14, -100,
            12, 9, 7, 4, 7, 4, 2, 0,     2, 4, 2, 0, -3, 0, 0, -100
        };

        // Root note (semitones from C3) per bar, played on quarter notes.
        private static readonly int[] BassRoots = { 0, 5, 9, 7, 0, 5, 7, 0 };

        public static AudioClip Build()
        {
            float eighth = 30f / Bpm;
            int length = Mathf.CeilToInt(Melody.Length * eighth * ToneSynth.SampleRate);
            var mix = new float[length];

            for (int i = 0; i < Melody.Length; i++)
            {
                if (Melody[i] > -100) AddNote(mix, i * eighth, eighth * 0.95f, Frequency(72 + Melody[i]), MelodyVolume, 0.25f);
            }
            for (int bar = 0; bar < BassRoots.Length; bar++)
            {
                for (int beat = 0; beat < 4; beat++)
                {
                    int note = 48 + BassRoots[bar] + (beat % 2 == 1 ? 7 : 0);
                    AddNote(mix, (bar * 8 + beat * 2) * eighth, eighth * 1.6f, Frequency(note), BassVolume, 0.5f);
                }
            }

            var clip = AudioClip.Create("Music", length, 1, ToneSynth.SampleRate, false);
            clip.SetData(mix, 0);
            return clip;
        }

        private static float Frequency(int midiNote) => 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);

        private static void AddNote(float[] mix, float start, float seconds, float hz, float volume, float square)
        {
            int from = Mathf.RoundToInt(start * ToneSynth.SampleRate);
            int count = Mathf.RoundToInt(seconds * ToneSynth.SampleRate);
            for (int i = 0; i < count && from + i < mix.Length; i++)
            {
                float t = (float)i / ToneSynth.SampleRate;
                float sine = Mathf.Sin(2f * Mathf.PI * hz * t);
                float wave = Mathf.Lerp(sine, Mathf.Sign(sine), square);
                float envelope = Mathf.Clamp01(i / 200f) * Mathf.Pow(1f - (float)i / count, 1.5f);
                mix[from + i] += wave * volume * envelope;
            }
        }
    }
}
