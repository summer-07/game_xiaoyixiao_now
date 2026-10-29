using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Enter and exit animation for a UI element: it fades through a CanvasGroup and either pops (via
    /// <see cref="ScaleAnimator"/>) or slides in from an edge, optionally after a delay so groups can stagger.
    /// Hiding plays the exit and only then deactivates the object. Runs on unscaled time so it works while paused.
    /// Only position and alpha are touched here, so it never fights ScaleAnimator over the scale.
    /// </summary>
    public sealed class UiTransition : MonoBehaviour
    {
        public enum Motion
        {
            Fade,
            Pop,
            FromTop,
            FromBottom,
            FromLeft,
            FromRight
        }

        private const float InSeconds = 0.34f;
        private const float OutSeconds = 0.2f;
        private const float SlideDistance = 360f;
        private const float OutSlideFraction = 0.5f;

        private Motion _motion;
        private float _delay;
        private CanvasGroup _group;
        private RectTransform _rect;
        private Vector2 _home;
        private bool _hasHome;
        private float _elapsed;
        private bool _entering;
        private bool _animating;
        private bool _popped;

        public bool IsShown { get; private set; }

        /// <param name="delay">Seconds to wait before entering, to stagger a group.</param>
        public static UiTransition On(Component target, Motion motion, float delay = 0f)
        {
            var transition = target.GetComponent<UiTransition>();
            if (transition == null) transition = target.gameObject.AddComponent<UiTransition>();
            transition._motion = motion;
            transition._delay = delay;
            transition.IsShown = target.gameObject.activeSelf;
            return transition;
        }

        public void Show()
        {
            Prepare();
            gameObject.SetActive(true);
            IsShown = true;
            _entering = true;
            _animating = true;
            _popped = false;
            _elapsed = -_delay;
            _group.blocksRaycasts = true;
            Apply(0f);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf || (!IsShown && _animating)) return;
            Prepare();
            IsShown = false;
            _entering = false;
            _animating = true;
            _elapsed = 0f;
            _group.blocksRaycasts = false;
        }

        /// <summary>Gone this frame, no exit animation (e.g. when the home screen takes over).</summary>
        public void HideImmediately()
        {
            Prepare();
            IsShown = false;
            _animating = false;
            Finish();
        }

        private void Prepare()
        {
            if (_group == null) _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
            _rect = (RectTransform)transform;
            if (!_hasHome)
            {
                _home = _rect.anchoredPosition;
                _hasHome = true;
            }
        }

        private void Update()
        {
            if (!_animating) return;
            _elapsed += Time.unscaledDeltaTime;
            float duration = _entering ? InSeconds : OutSeconds;
            float t = Mathf.Clamp01(_elapsed / duration);
            if (_entering && _elapsed >= 0f && !_popped && _motion == Motion.Pop)
            {
                ScaleAnimator.On(this).PopIn();
                _popped = true;
            }
            Apply(_elapsed < 0f ? 0f : t);
            if (t < 1f) return;

            _animating = false;
            if (!_entering) Finish();
        }

        private void Apply(float t)
        {
            if (_entering)
            {
                _group.alpha = Mathf.SmoothStep(0f, 1f, t);
                _rect.anchoredPosition = _home + Offset() * (1f - EaseOutBack(t));
            }
            else
            {
                _group.alpha = 1f - t;
                _rect.anchoredPosition = _home + Offset() * (t * t * OutSlideFraction);
            }
        }

        private void Finish()
        {
            gameObject.SetActive(false);
            if (_group != null) _group.alpha = 1f;
            if (_rect != null && _hasHome) _rect.anchoredPosition = _home;
        }

        private Vector2 Offset()
        {
            switch (_motion)
            {
                case Motion.FromTop: return Vector2.up * SlideDistance;
                case Motion.FromBottom: return Vector2.down * SlideDistance;
                case Motion.FromLeft: return Vector2.left * SlideDistance;
                case Motion.FromRight: return Vector2.right * SlideDistance;
                default: return Vector2.zero;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float overshoot = 1.4f;
            float p = t - 1f;
            return 1f + (overshoot + 1f) * p * p * p + overshoot * p * p;
        }
    }
}
