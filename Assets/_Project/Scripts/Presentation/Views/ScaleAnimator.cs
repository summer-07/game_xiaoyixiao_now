using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Owns one transform's uniform scale and layers juice on top of a base scale: an elastic punch,
    /// an overshooting pop-in, a held "pressed" squash and an idle breathing loop with a gentle wiggle.
    /// Only writes the transform while something moves.
    /// </summary>
    public sealed class ScaleAnimator : MonoBehaviour
    {
        private const float PunchSeconds = 0.3f;
        private const float PunchWobbles = 2.5f;
        private const float PopInSeconds = 0.32f;
        private const float BackOvershoot = 1.9f;
        private const float PressSpeed = 25f;

        private float _baseScale = 1f;
        private float _pressTarget = 1f;
        private float _press = 1f;
        private float _punchAmount;
        private float _punchElapsed = PunchSeconds;
        private float _popElapsed = PopInSeconds;
        private bool _dirty = true;
        private float _breatheAmount;
        private float _breatheSpeed;
        private float _breathePhase;
        private float _wiggleDegrees;

        /// <remarks>No "??": a missing component is a fake-null Unity object in the editor.</remarks>
        public static ScaleAnimator On(Component target)
        {
            var existing = target.GetComponent<ScaleAnimator>();
            return existing != null ? existing : target.gameObject.AddComponent<ScaleAnimator>();
        }

        public float BaseScale
        {
            get => _baseScale;
            set
            {
                _baseScale = value;
                _dirty = true;
            }
        }

        public void Punch(float amount)
        {
            _punchAmount = amount;
            _punchElapsed = 0f;
        }

        public void PopIn()
        {
            _popElapsed = 0f;
            transform.localScale = Vector3.zero;
        }

        public void Hold(float squash) => _pressTarget = squash;

        /// <summary>Idle loop so resting elements still feel alive. Pass 0 to stop.</summary>
        /// <param name="phase">Offsets neighbours so a row of buttons doesn't pulse in lockstep.</param>
        public void Breathe(float amount, float cyclesPerSecond, float phase, float wiggleDegrees = 0f)
        {
            _breatheAmount = amount;
            _breatheSpeed = cyclesPerSecond * Mathf.PI * 2f;
            _breathePhase = phase;
            _wiggleDegrees = wiggleDegrees;
            _dirty = true;
        }

        public void Release(float punch)
        {
            _pressTarget = 1f;
            Punch(punch);
        }

        private void OnDisable()
        {
            _press = _pressTarget = 1f;
            _punchElapsed = PunchSeconds;
            _popElapsed = PopInSeconds;
            transform.localScale = Vector3.one * _baseScale;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool moving = _breatheAmount > 0f || _punchElapsed < PunchSeconds || _popElapsed < PopInSeconds || !Mathf.Approximately(_press, _pressTarget);
            if (!moving && !_dirty) return;

            _punchElapsed = Mathf.Min(PunchSeconds, _punchElapsed + dt);
            _popElapsed = Mathf.Min(PopInSeconds, _popElapsed + dt);
            _press = Mathf.MoveTowards(_press, _pressTarget, PressSpeed * dt);
            float wave = Mathf.Sin(Time.unscaledTime * _breatheSpeed + _breathePhase);
            transform.localScale = Vector3.one * (_baseScale * _press * PunchFactor() * PopFactor() * (1f + wave * _breatheAmount));
            if (_wiggleDegrees > 0f) transform.localRotation = Quaternion.Euler(0f, 0f, wave * _wiggleDegrees);
            _dirty = false;
        }

        private float PunchFactor()
        {
            float t = _punchElapsed / PunchSeconds;
            return 1f + _punchAmount * Mathf.Sin(t * Mathf.PI * PunchWobbles) * (1f - t);
        }

        private float PopFactor()
        {
            float t = _popElapsed / PopInSeconds - 1f;
            return 1f + (BackOvershoot + 1f) * t * t * t + BackOvershoot * t * t;
        }
    }
}
