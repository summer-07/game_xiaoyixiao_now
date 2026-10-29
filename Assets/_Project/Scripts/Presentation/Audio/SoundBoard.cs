using UnityEngine;
using static SheepStrike.Presentation.Audio.ToneSynth;

namespace SheepStrike.Presentation.Audio
{
    /// <summary>
    /// All game sound effects. Plays through a small round-robin pool of sources so each shot keeps its own
    /// pitch, and throttles the rapid-fire sounds so a big combo stays crisp instead of turning into noise.
    /// </summary>
    public sealed class SoundBoard : MonoBehaviour
    {
        private const int Voices = 10;
        private const float FireCooldown = 0.045f;
        private const float HitCooldown = 0.035f;
        private const float PitchJitter = 0.06f;
        private const float HitPitchPerTier = 0.08f;

        private AudioSource[] _sources;
        private int _nextSource;
        private float _lastFire = -1f;
        private float _lastHit = -1f;

        private AudioClip _tap;
        private AudioClip _hop;
        private AudioClip _land;
        private AudioClip _fire;
        private AudioClip _hit;
        private AudioClip _denied;
        private AudioClip _booster;
        private AudioClip _win;
        private AudioClip _lose;
        private AudioClip[] _fanfares;

        public void Init()
        {
            _sources = new AudioSource[Voices];
            for (int i = 0; i < Voices; i++)
            {
                _sources[i] = gameObject.AddComponent<AudioSource>();
                _sources[i].playOnAwake = false;
            }
            BuildClips();
        }

        public void Tap() => Play(_tap, 1f);

        public void Hop() => Play(_hop, Jitter());

        public void Land() => Play(_land, Jitter());

        public void Denied() => Play(_denied, 1f);

        public void Booster() => Play(_booster, 1f);

        public void Win() => Play(_win, 1f);

        public void Lose() => Play(_lose, 1f);

        public void Fire()
        {
            if (Time.unscaledTime - _lastFire < FireCooldown) return;
            _lastFire = Time.unscaledTime;
            Play(_fire, Jitter());
        }

        /// <param name="comboTier">Each tier lifts the pop's pitch, so a growing combo audibly climbs.</param>
        public void Hit(int comboTier)
        {
            if (Time.unscaledTime - _lastHit < HitCooldown) return;
            _lastHit = Time.unscaledTime;
            Play(_hit, Jitter() + comboTier * HitPitchPerTier);
        }

        /// <param name="tier">1..ComboTier.Count.</param>
        public void Fanfare(int tier) => Play(_fanfares[Mathf.Clamp(tier - 1, 0, _fanfares.Length - 1)], 1f);

        private static float Jitter() => 1f + Random.Range(-PitchJitter, PitchJitter);

        private void Play(AudioClip clip, float pitch)
        {
            var source = _sources[_nextSource];
            _nextSource = (_nextSource + 1) % _sources.Length;
            source.pitch = pitch;
            source.PlayOneShot(clip);
        }

        private void BuildClips()
        {
            _tap = Clip("Tap", Sweep(1100f, 700f, 0.05f, 0.35f));
            _hop = Clip("Hop", Sweep(260f, 620f, 0.14f, 0.4f));
            _land = Clip("Land", new Tone { StartHz = 180f, EndHz = 90f, Seconds = 0.09f, Volume = 0.5f, Noise = 0.15f });
            _fire = Clip("Fire", new Tone { StartHz = 900f, EndHz = 300f, Seconds = 0.07f, Volume = 0.22f, Noise = 0.35f });
            _hit = Clip("Hit", new Tone { StartHz = 520f, EndHz = 140f, Seconds = 0.11f, Volume = 0.45f, Noise = 0.45f });
            _denied = Clip("Denied", new Tone { StartHz = 220f, EndHz = 160f, Seconds = 0.18f, Volume = 0.25f, Square = 0.6f });
            _booster = Clip("Booster", Sweep(523f, 523f, 0.07f, 0.3f), Sweep(784f, 784f, 0.07f, 0.3f), Sweep(1047f, 1400f, 0.18f, 0.3f));
            _win = Clip("Win", Sweep(523f, 523f, 0.12f, 0.35f), Sweep(659f, 659f, 0.12f, 0.35f), Sweep(784f, 784f, 0.12f, 0.35f), Sweep(1047f, 1047f, 0.4f, 0.35f));
            _lose = Clip("Lose", Sweep(392f, 392f, 0.18f, 0.3f), Sweep(330f, 330f, 0.18f, 0.3f), Sweep(262f, 220f, 0.45f, 0.3f));
            _fanfares = new[]
            {
                Clip("Combo1", Sweep(659f, 659f, 0.08f, 0.3f), Sweep(988f, 988f, 0.2f, 0.3f)),
                Clip("Combo2", Sweep(659f, 659f, 0.07f, 0.3f), Sweep(784f, 784f, 0.07f, 0.3f), Sweep(1175f, 1175f, 0.22f, 0.3f)),
                Clip("Combo3", Sweep(523f, 523f, 0.06f, 0.32f), Sweep(659f, 659f, 0.06f, 0.32f), Sweep(784f, 784f, 0.06f, 0.32f), Sweep(1319f, 1319f, 0.26f, 0.32f)),
                Clip("Combo4", Sweep(523f, 523f, 0.06f, 0.35f), Sweep(784f, 784f, 0.06f, 0.35f), Sweep(1047f, 1047f, 0.06f, 0.35f), Sweep(1568f, 2093f, 0.35f, 0.35f))
            };
        }
    }
}
